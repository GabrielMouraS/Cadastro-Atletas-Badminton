$ErrorActionPreference = "Stop"
$projetoDir = Join-Path $PSScriptRoot "Cadastro-Atletas-Badminton"
$publishDir = Join-Path $PSScriptRoot "publish"
$csproj     = Join-Path $projetoDir "BadmintonCadastro.csproj"
$iscc       = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$iss        = Join-Path $PSScriptRoot "setup.iss"

Write-Host "==> Publicando..." -ForegroundColor Cyan
dotnet publish $csproj -c Release -r win-x64 --self-contained `
    -p:PublishSingleFile=true -p:PublishTrimmed=false `
    -o $publishDir

if ($LASTEXITCODE -ne 0) { throw "dotnet publish falhou." }

Write-Host "==> Publish concluído em: $publishDir" -ForegroundColor Green

if (Test-Path $iscc) {
    Write-Host "==> Gerando instalador..." -ForegroundColor Cyan
    & $iscc $iss
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup falhou." }
    Write-Host "==> Instalador gerado em: $PSScriptRoot\installer\" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "[AVISO] Inno Setup nao encontrado em: $iscc" -ForegroundColor Yellow
    Write-Host "        Baixe em https://jrsoftware.org/isinfo.php" -ForegroundColor Yellow
    Write-Host "        Depois execute: & '$iscc' '$iss'" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "        Os arquivos publicados estao prontos em: $publishDir" -ForegroundColor White
}
