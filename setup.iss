[Setup]
AppName=Badminton Cadastro
AppVersion=1.0
AppPublisher=Badminton
DefaultDirName={autopf}\BadmintonCadastro
DefaultGroupName=Badminton Cadastro
OutputDir=.\installer
OutputBaseFilename=BadmintonCadastro_Setup_v1.0
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
DisableProgramGroupPage=yes
UninstallDisplayName=Badminton Cadastro
UninstallDisplayIcon={app}\BadmintonCadastro.exe

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "publish\BadmintonCadastro.exe";              DestDir: "{app}"; Flags: ignoreversion
Source: "publish\e_sqlite3.dll";                      DestDir: "{app}"; Flags: ignoreversion
Source: "publish\libSkiaSharp.dll";                   DestDir: "{app}"; Flags: ignoreversion
Source: "publish\Assets\ModeloPlanilha\modelo.xlsx";  DestDir: "{app}\Assets\ModeloPlanilha"; Flags: ignoreversion

[Icons]
Name: "{group}\Badminton Cadastro";    Filename: "{app}\BadmintonCadastro.exe"
Name: "{commondesktop}\Badminton Cadastro"; Filename: "{app}\BadmintonCadastro.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Ícones adicionais:"

[Run]
Filename: "{app}\BadmintonCadastro.exe"; Description: "Abrir Badminton Cadastro"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; O banco de dados fica em %LOCALAPPDATA%\BadmintonCadastro\ — NÃO é removido no uninstall (preserva os dados do usuário)
