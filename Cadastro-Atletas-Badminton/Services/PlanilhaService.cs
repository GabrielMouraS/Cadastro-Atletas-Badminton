using System.IO.Compression;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using BadmintonCadastro.Models;

namespace BadmintonCadastro.Services;

public enum TipoExportPlanilha
{
    SimplesDuplas,            // ESTADUAL / INTERESCOLAR  → abas SIMPLES + DUPLAS
    RegionalClassificatorio,  // REGIONAL classificatório → aba REGIONAL CLASSIFICATÓRIO
    RegionalAmistoso          // REGIONAL amistoso        → aba REGIONAL AMISTOSO
}

internal static class PlanilhaService
{
    private static readonly string ModeloPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "Assets", "ModeloPlanilha", "modelo.xlsx");

    // Índices de aba (0-based)
    private const int SheetSimples          = 1;
    private const int SheetDuplas           = 2;
    private const int SheetRegionalClass    = 3;
    private const int SheetRegionalAmistoso = 4;

    // Colunas comuns — atleta 1 (0-based: A=0, B=1, ...)
    private const int ColId    = 1;  // B - ID / codigo_federacao
    private const int ColNome  = 2;  // C - nome_completo
    private const int ColClube = 3;  // D - sigla da entidade
    private const int ColAno   = 4;  // E - ano_nascimento
    private const int ColSexo  = 5;  // F - sexo (M/F)
    private const int ColCat   = 6;  // G - label da categoria (ex: SMSub11)

    // Colunas DUPLAS — atleta 2
    private const int ColId2    = 7;   // H - ID atleta2 / "ID DUPLA"
    private const int ColNome2  = 8;   // I - "Nome da dupla"
    private const int ColClube2 = 9;   // J - "Clube da dupla"
    private const int ColAno2   = 10;  // K - ano_nascimento atleta2
    private const int ColSexo2  = 11;  // L - sexo atleta2

    // Linhas de início dos dados (0-based = Excel row - 1)
    private const int SimplesStartRow      = 10;  // Excel row 11
    private const int DuplasStartRow       = 10;  // Excel row 11
    private const int RegClassStartRow     = 11;  // Excel row 12
    private const int RegAmistFedStart     = 11;  // Excel row 12 (federados)
    private const int RegAmistNaFedStart   = 29;  // Excel row 30 (não-federados)

    // Limites de linhas (slots disponíveis)
    private const int SimplesMaxRows    = 30;
    private const int DuplasMaxRows     = 82;
    private const int RegClassMaxRows   = 15;
    private const int RegAmistFedMax    = 12;
    private const int RegAmistNaFedMax  = 12;

    // Linha do cabeçalho de torneio/clube nas abas regionais (0-based)
    private const int RegHeaderTorneioRow = 5;  // Excel row 6  — label "Torneio:"
    private const int RegHeaderClubeRow   = 6;  // Excel row 7  — label "Clube:"
    private const int RegHeaderValueCol   = 2;  // C — célula ao lado do label

    // ─── Ponto de entrada público ────────────────────────────────────────────

    public static void ExportarFicha(
        Torneio torneio,
        Entidade? entidade,      // null = todas as entidades
        TipoExportPlanilha tipoExport,
        string destinoPath)
    {
        if (!File.Exists(ModeloPath))
            throw new FileNotFoundException(
                $"Modelo não encontrado: {ModeloPath}\n" +
                "Verifique se 'Assets/ModeloPlanilha/modelo.xlsx' está presente e marcado como 'Copy to Output Directory'.");

        int anoRef = torneio.DataInicio?.Year ?? DateTime.Now.Year;

        // Carrega inscrições com JOINs (Atleta1, Atleta2, Categoria já populados)
        var inscricoes = InscricoesService.ListarPorTorneio(torneio.Id).ToList();

        // Dicionário de atletas com Entidade para resolução rápida
        var atletasDict = AtletasService.ListarComEntidade().ToDictionary(a => a.Id);

        // Carrega cópia do modelo em memória — NUNCA abre o original para escrita
        var modeloBytes = File.ReadAllBytes(ModeloPath);
        using var memStream = new MemoryStream(modeloBytes);
        using var wb = new XSSFWorkbook(memStream);

        switch (tipoExport)
        {
            case TipoExportPlanilha.SimplesDuplas:
                EscreverSimples(wb, entidade, inscricoes, atletasDict, anoRef);
                EscreverDuplas(wb, entidade, inscricoes, atletasDict, anoRef);
                break;

            case TipoExportPlanilha.RegionalClassificatorio:
                EscreverRegionalClass(wb, torneio, entidade, inscricoes, atletasDict, anoRef);
                break;

            case TipoExportPlanilha.RegionalAmistoso:
                EscreverRegionalAmistoso(wb, torneio, entidade, inscricoes, atletasDict, anoRef);
                break;
        }

        var dir = Path.GetDirectoryName(destinoPath);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        // NPOI escreve os dados mas corrompe drawings/tema do modelo.
        // Solução: escrever em memória e depois restaurar as entradas originais via ZIP.
        using var outMem = new MemoryStream();
        wb.Write(outMem, leaveOpen: true);
        var resultado = RestaurarEntradasModelo(modeloBytes, outMem.ToArray());
        File.WriteAllBytes(destinoPath, resultado);
    }

    // ─── SIMPLES ─────────────────────────────────────────────────────────────

    private static void EscreverSimples(
        XSSFWorkbook wb, Entidade? entidade,
        List<Inscricao> inscricoes, Dictionary<int, Atleta> atletasDict, int anoRef)
    {
        var sheet = wb.GetSheetAt(SheetSimples);

        LimparSecao(sheet, SimplesStartRow, SimplesMaxRows, ColId, ColCat);

        var simples = inscricoes
            .Where(i => i.Categoria?.Tipo == "SIMPLES"
                     && (entidade == null ||
                         Resolve(i.Atleta1Id, i.Atleta1, atletasDict).EntidadeId == entidade.Id))
            .ToList();

        int row = SimplesStartRow;
        foreach (var insc in simples.Take(SimplesMaxRows))
        {
            var a = Resolve(insc.Atleta1Id, insc.Atleta1, atletasDict);
            EscreverAtleta(sheet, row++, a, ComputarLabel(a, insc.Categoria!, anoRef));
        }
    }

    // ─── DUPLAS ──────────────────────────────────────────────────────────────

    private static void EscreverDuplas(
        XSSFWorkbook wb, Entidade? entidade,
        List<Inscricao> inscricoes, Dictionary<int, Atleta> atletasDict, int anoRef)
    {
        var sheet = wb.GetSheetAt(SheetDuplas);

        LimparSecao(sheet, DuplasStartRow, DuplasMaxRows, ColId, ColSexo2);

        var duplas = inscricoes
            .Where(i => i.Categoria?.Tipo is "DUPLA" or "MISTA"
                     && (entidade == null
                      || Resolve(i.Atleta1Id, i.Atleta1, atletasDict).EntidadeId == entidade.Id
                      || (i.Atleta2Id.HasValue
                          && Resolve(i.Atleta2Id.Value, i.Atleta2, atletasDict).EntidadeId == entidade.Id)))
            .ToList();

        int row = DuplasStartRow;
        foreach (var insc in duplas.Take(DuplasMaxRows))
        {
            var a1 = Resolve(insc.Atleta1Id, insc.Atleta1, atletasDict);
            EscreverAtleta(sheet, row, a1, ComputarLabel(a1, insc.Categoria!, anoRef));

            if (insc.Atleta2Id.HasValue)
            {
                var a2 = Resolve(insc.Atleta2Id.Value, insc.Atleta2, atletasDict);
                SetStr(sheet, row, ColId2,    a2.CodigoFederacao ?? string.Empty);
                SetStr(sheet, row, ColNome2,  $"{a1.NomeCompleto} / {a2.NomeCompleto}");
                SetStr(sheet, row, ColClube2, a2.Entidade?.Sigla ?? string.Empty);
                SetNum(sheet, row, ColAno2,   a2.AnoNascimento);
                SetStr(sheet, row, ColSexo2,  a2.Sexo);
            }
            row++;
        }
    }

    // ─── REGIONAL CLASSIFICATÓRIO ────────────────────────────────────────────

    private static void EscreverRegionalClass(
        XSSFWorkbook wb, Torneio torneio, Entidade? entidade,
        List<Inscricao> inscricoes, Dictionary<int, Atleta> atletasDict, int anoRef)
    {
        var sheet = wb.GetSheetAt(SheetRegionalClass);

        SetStr(sheet, RegHeaderTorneioRow, RegHeaderValueCol, torneio.Nome);
        SetStr(sheet, RegHeaderClubeRow,   RegHeaderValueCol, entidade?.Sigla ?? "Todas");

        LimparSecao(sheet, RegClassStartRow, RegClassMaxRows, ColId, ColSexo2);

        var insc = inscricoes
            .Where(i => entidade == null
                     || Resolve(i.Atleta1Id, i.Atleta1, atletasDict).EntidadeId == entidade.Id
                     || (i.Atleta2Id.HasValue
                         && Resolve(i.Atleta2Id.Value, i.Atleta2, atletasDict).EntidadeId == entidade.Id))
            .ToList();

        int row = RegClassStartRow;
        foreach (var i in insc.Take(RegClassMaxRows))
        {
            var a1 = Resolve(i.Atleta1Id, i.Atleta1, atletasDict);
            EscreverAtleta(sheet, row, a1, ComputarLabel(a1, i.Categoria!, anoRef));

            if (i.Atleta2Id.HasValue)
                EscreverAtleta2(sheet, row, i, atletasDict, a1);

            row++;
        }
    }

    // ─── REGIONAL AMISTOSO ───────────────────────────────────────────────────

    private static void EscreverRegionalAmistoso(
        XSSFWorkbook wb, Torneio torneio, Entidade? entidade,
        List<Inscricao> inscricoes, Dictionary<int, Atleta> atletasDict, int anoRef)
    {
        var sheet = wb.GetSheetAt(SheetRegionalAmistoso);

        SetStr(sheet, RegHeaderTorneioRow, RegHeaderValueCol, torneio.Nome);
        SetStr(sheet, RegHeaderClubeRow,   RegHeaderValueCol, entidade?.Sigla ?? "Todas");

        LimparSecao(sheet, RegAmistFedStart,   RegAmistFedMax,   ColId, ColSexo2);
        LimparSecao(sheet, RegAmistNaFedStart, RegAmistNaFedMax, ColId, ColSexo2);

        var minhas = inscricoes
            .Where(i => entidade == null
                     || Resolve(i.Atleta1Id, i.Atleta1, atletasDict).EntidadeId == entidade.Id
                     || (i.Atleta2Id.HasValue
                         && Resolve(i.Atleta2Id.Value, i.Atleta2, atletasDict).EntidadeId == entidade.Id))
            .ToList();

        // Federados: atleta1 tem codigo_federacao
        var federados    = minhas.Where(i => Resolve(i.Atleta1Id, i.Atleta1, atletasDict).CodigoFederacao != null).ToList();
        var naoFederados = minhas.Where(i => Resolve(i.Atleta1Id, i.Atleta1, atletasDict).CodigoFederacao == null).ToList();

        EscreverSecaoAmistoso(sheet, federados,    atletasDict, anoRef, RegAmistFedStart,   RegAmistFedMax);
        EscreverSecaoAmistoso(sheet, naoFederados, atletasDict, anoRef, RegAmistNaFedStart, RegAmistNaFedMax);
    }

    private static void EscreverSecaoAmistoso(
        ISheet sheet, List<Inscricao> inscricoes, Dictionary<int, Atleta> atletasDict,
        int anoRef, int startRow, int maxRows)
    {
        int row = startRow;
        foreach (var i in inscricoes.Take(maxRows))
        {
            var a1 = Resolve(i.Atleta1Id, i.Atleta1, atletasDict);
            EscreverAtleta(sheet, row, a1, ComputarLabel(a1, i.Categoria!, anoRef));

            if (i.Atleta2Id.HasValue)
                EscreverAtleta2(sheet, row, i, atletasDict, a1);

            row++;
        }
    }

    // ─── Helpers internos ────────────────────────────────────────────────────

    // Apaga células de dados (sem tocar fórmulas) para não deixar linhas de exemplo do template
    private static void LimparSecao(ISheet sheet, int startRow, int maxRows, int firstCol, int lastCol)
    {
        for (int r = startRow; r < startRow + maxRows; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;
            for (int c = firstCol; c <= lastCol; c++)
            {
                var cell = row.GetCell(c);
                if (cell == null || cell.CellType == CellType.Formula) continue;
                // SetCellType(Blank) é bugado no NPOI XSSF; limpar com valor vazio é mais seguro
                if (cell.CellType == CellType.Numeric)
                    cell.SetCellValue(0d);
                else
                    cell.SetCellValue(string.Empty);
            }
        }
    }

    private static void EscreverAtleta(ISheet sheet, int rowIdx, Atleta atleta, string label)
    {
        SetStr(sheet, rowIdx, ColId,    atleta.CodigoFederacao ?? string.Empty);
        SetStr(sheet, rowIdx, ColNome,  atleta.NomeCompleto);
        SetStr(sheet, rowIdx, ColClube, atleta.Entidade?.Sigla ?? string.Empty);
        SetNum(sheet, rowIdx, ColAno,   atleta.AnoNascimento);
        SetStr(sheet, rowIdx, ColSexo,  atleta.Sexo);
        SetStr(sheet, rowIdx, ColCat,   label);
    }

    private static void EscreverAtleta2(
        ISheet sheet, int rowIdx, Inscricao insc,
        Dictionary<int, Atleta> atletasDict, Atleta a1)
    {
        var a2 = Resolve(insc.Atleta2Id!.Value, insc.Atleta2, atletasDict);
        SetStr(sheet, rowIdx, ColId2,    a2.CodigoFederacao ?? string.Empty);
        SetStr(sheet, rowIdx, ColNome2,  $"{a1.NomeCompleto} / {a2.NomeCompleto}");
        SetStr(sheet, rowIdx, ColClube2, a2.Entidade?.Sigla ?? string.Empty);
        SetNum(sheet, rowIdx, ColAno2,   a2.AnoNascimento);
        SetStr(sheet, rowIdx, ColSexo2,  a2.Sexo);
    }

    // Gravação segura: nunca sobrescreve fórmulas
    private static void SetStr(ISheet sheet, int rowIdx, int colIdx, string value)
    {
        var row  = sheet.GetRow(rowIdx) ?? sheet.CreateRow(rowIdx);
        var cell = row.GetCell(colIdx);
        if (cell?.CellType == CellType.Formula) return;
        (cell ?? row.CreateCell(colIdx)).SetCellValue(value);
    }

    private static void SetNum(ISheet sheet, int rowIdx, int colIdx, double value)
    {
        var row  = sheet.GetRow(rowIdx) ?? sheet.CreateRow(rowIdx);
        var cell = row.GetCell(colIdx);
        if (cell?.CellType == CellType.Formula) return;
        (cell ?? row.CreateCell(colIdx)).SetCellValue(value);
    }

    // Retorna o atleta do dicionário (com Entidade) ou usa o objeto do JOIN como fallback
    private static Atleta Resolve(int atletaId, Atleta? fromJoin, Dictionary<int, Atleta> dict) =>
        dict.TryGetValue(atletaId, out var full) ? full
        : fromJoin ?? new Atleta { Id = atletaId, NomeCompleto = $"Atleta#{atletaId}" };

    // ─── Label de categoria (coluna G) ───────────────────────────────────────

    // Código de categoria que corresponde exatamente ao Codigo no banco
    // Ex: SM09, DF13, DXA, SM35
    private static string ComputarLabel(Atleta atleta, Categoria categoria, int anoRef)
    {
        string faixa = atleta.AnoNascimento switch
        {
            var a when a >= anoRef - 8  => "09",
            var a when a >= anoRef - 10 => "11",
            var a when a >= anoRef - 12 => "13",
            var a when a >= anoRef - 14 => "15",
            var a when a >= anoRef - 16 => "17",
            var a when a >= anoRef - 18 => "19",
            var a when a >  anoRef - 35 => "A",
            _                           => "35"
        };

        string prefixo = categoria.Tipo switch
        {
            "SIMPLES" => atleta.Sexo == "M" ? "SM" : "SF",
            "DUPLA"   => atleta.Sexo == "M" ? "DM" : "DF",
            "MISTA"   => "DX",
            _         => atleta.Sexo == "M" ? "SM" : "SF"
        };

        return prefixo + faixa;
    }

    // ─── Restaura entradas que o NPOI corrompe ao reescrever ─────────────────
    // O NPOI reescreve o XLSX inteiro mas não sabe lidar com drawings e temas.
    // Solução: montar um novo ZIP copiando tudo do output do NPOI, mas
    // substituindo drawings/tema/media pelas entradas originais do modelo.

    private static readonly string[] _prefixosModelo =
        ["xl/drawings/", "xl/theme/", "xl/media/"];

    private static byte[] RestaurarEntradasModelo(byte[] modeloBytes, byte[] npoiBytes)
    {
        using var modeloZip = new ZipArchive(new MemoryStream(modeloBytes), ZipArchiveMode.Read);
        using var npoiZip   = new ZipArchive(new MemoryStream(npoiBytes),   ZipArchiveMode.Read);
        using var saida     = new MemoryStream();

        // Entradas do modelo que devem sobrescrever a versão do NPOI
        var deModelo = modeloZip.Entries
            .Where(e => _prefixosModelo.Any(p => e.FullName.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            .ToDictionary(e => e.FullName, StringComparer.OrdinalIgnoreCase);

        using (var saidaZip = new ZipArchive(saida, ZipArchiveMode.Create, leaveOpen: true))
        {
            // 1. Tudo do NPOI, exceto o que o modelo vai prover
            foreach (var e in npoiZip.Entries)
            {
                if (deModelo.ContainsKey(e.FullName)) continue;
                CopiarEntrada(e, saidaZip);
            }

            // 2. Entradas originais do modelo (drawings, tema, media)
            foreach (var e in deModelo.Values)
                CopiarEntrada(e, saidaZip);
        }

        return saida.ToArray();
    }

    private static void CopiarEntrada(ZipArchiveEntry origem, ZipArchive destino)
    {
        var nova = destino.CreateEntry(origem.FullName, CompressionLevel.Optimal);
        using var src = origem.Open();
        using var dst = nova.Open();
        src.CopyTo(dst);
    }
}
