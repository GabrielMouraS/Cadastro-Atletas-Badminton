using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace BadmintonCadastro.UI.Forms;

internal class ImportarAtletasForm : Form
{
    private readonly TextBox _txtArquivo  = new() { ReadOnly = true, Dock = DockStyle.Fill };
    private readonly Label   _lblStatus   = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoSize = false };
    private readonly Button  _btnImportar;
    private string? _arquivoPath;

    // Colunas 0-based: B=1 C=2 D=3 E=4 F=5  —  linha inicial 0-based = 14 (Excel row 15)
    private const int StartRow  = 14;
    private const int ColIdAtl  = 1;
    private const int ColNome   = 2;
    private const int ColClube  = 3;
    private const int ColAno    = 4;
    private const int ColSexo   = 5;

    public ImportarAtletasForm()
    {
        Text            = "Importar Atletas de Planilha";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition   = FormStartPosition.CenterParent;
        ClientSize      = new Size(540, 190);
        BackColor       = Estilos.ContentBg;

        _btnImportar         = Estilos.CriarBotao("Importar", "novo");
        _btnImportar.Enabled = false;
        _btnImportar.Width   = 110;

        var btnBrowse = new Button
        {
            Text      = "Escolher arquivo…",
            Dock      = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Estilos.AccentBlue,
            ForeColor = Color.White,
            Cursor    = Cursors.Hand,
        };
        btnBrowse.FlatAppearance.BorderSize = 0;
        btnBrowse.Click += BtnBrowse_Click;

        var btnCancelar = Estilos.CriarBotao("Cancelar", "padrao");
        btnCancelar.DialogResult = DialogResult.Cancel;

        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Padding     = new Padding(16),
            ColumnCount = 2,
            RowCount    = 3,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent,  100));

        layout.Controls.Add(_txtArquivo, 0, 0);
        layout.Controls.Add(btnBrowse,   1, 0);

        _lblStatus.Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f);
        _lblStatus.ForeColor = Estilos.TextPrimary;
        _lblStatus.Text      = "Selecione a planilha de atletas (.xlsx).";
        layout.Controls.Add(_lblStatus, 0, 1);
        layout.SetColumnSpan(_lblStatus, 2);

        var btnFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents  = false,
        };
        btnFlow.Controls.AddRange(new Control[] { btnCancelar, _btnImportar });
        layout.Controls.Add(btnFlow, 0, 2);
        layout.SetColumnSpan(btnFlow, 2);

        Controls.Add(layout);
        CancelButton         = btnCancelar;
        _btnImportar.Click  += BtnImportar_Click;
    }

    private void BtnBrowse_Click(object? s, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Title  = "Selecionar planilha de atletas",
            Filter = "Planilha Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        _arquivoPath         = dlg.FileName;
        _txtArquivo.Text     = dlg.FileName;
        _lblStatus.Text      = "Pronto. Clique em Importar para iniciar.";
        _lblStatus.ForeColor = Estilos.AccentBlue;
        _btnImportar.Enabled = true;
    }

    private void BtnImportar_Click(object? s, EventArgs e)
    {
        if (_arquivoPath == null) return;
        _btnImportar.Enabled = false;
        _lblStatus.ForeColor = Estilos.TextPrimary;
        _lblStatus.Text      = "Lendo planilha e importando…";
        Application.DoEvents();

        try
        {
            var (inseridos, erros, entidades) = LerEImportar(_arquivoPath);

            _lblStatus.ForeColor = erros == 0 ? Estilos.AccentGreen : Color.DarkOrange;
            _lblStatus.Text      = $"{inseridos} atleta(s) inserido(s) · {entidades} entidade(s) criada(s) · {erros} linha(s) ignorada(s).";

            MessageBox.Show(
                $"Importação concluída!\n\n" +
                $"• {inseridos} atleta(s) inserido(s)\n" +
                $"• {entidades} entidade(s) criada(s)\n" +
                $"• {erros} linha(s) ignorada(s) (dados inválidos ou duplicata)",
                "Importação concluída",
                MessageBoxButtons.OK,
                erros > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            _lblStatus.ForeColor = Color.Red;
            _lblStatus.Text      = $"Erro: {ex.Message}";
            _btnImportar.Enabled = true;
        }
    }

    private static (int inseridos, int erros, int entidades) LerEImportar(string path)
    {
        using var stream = File.OpenRead(path);
        IWorkbook wb = path.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
            ? new HSSFWorkbook(stream)
            : new XSSFWorkbook(stream);
        var sheet = wb.GetSheetAt(0);

        // Cache entidades: nome_completo → id  +  siglas já usadas para evitar colisão
        var existentes = EntidadesService.Listar().ToList();
        var entCache   = existentes.ToDictionary(e => e.NomeCompleto.Trim(), e => e.Id, StringComparer.OrdinalIgnoreCase);
        var siglasUsadas = existentes.Select(e => e.Sigla.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        int novasEnt = 0;

        var atletasParaInserir = new List<Atleta>();
        int erros = 0;

        for (int r = StartRow; r <= sheet.LastRowNum; r++)
        {
            var row = sheet.GetRow(r);
            if (row == null) continue;

            string nome   = CellStr(row, ColNome)?.Trim()  ?? "";
            string clube  = CellStr(row, ColClube)?.Trim() ?? "";
            string sexo   = (CellStr(row, ColSexo)?.Trim() ?? "").ToUpperInvariant();
            string codFed = CellStr(row, ColIdAtl)?.Trim() ?? "";
            int    ano    = CellInt(row, ColAno);

            if (string.IsNullOrEmpty(nome) || string.IsNullOrEmpty(clube)) continue;
            if (sexo != "M" && sexo != "F")  { erros++; continue; }
            if (ano < 1900 || ano > DateTime.Now.Year) { erros++; continue; }

            // Resolve ou cria entidade
            if (!entCache.TryGetValue(clube, out int entId))
            {
                // Gera sigla única: tenta base, depois base2, base3 ...
                string siglaBase = GerarSigla(clube);
                string sigla     = siglaBase;
                int    sufixo    = 2;
                while (siglasUsadas.Contains(sigla))
                    sigla = siglaBase + sufixo++;

                entId = EntidadesService.Inserir(new Entidade { NomeCompleto = clube, Sigla = sigla, Cidade = "" });
                entCache[clube] = entId;
                siglasUsadas.Add(sigla);
                novasEnt++;
            }

            atletasParaInserir.Add(new Atleta
            {
                EntidadeId      = entId,
                NomeCompleto    = nome,
                AnoNascimento   = ano,
                Sexo            = sexo,
                CodigoFederacao = string.IsNullOrEmpty(codFed) ? null : codFed,
            });
        }

        var (inseridos, errosInsert) = AtletasService.InserirLote(atletasParaInserir);
        return (inseridos, erros + errosInsert, novasEnt);
    }

    private static string GerarSigla(string clube)
    {
        // Usa a parte antes do primeiro " - " ou "-" como sigla, senão primeiras letras
        var idx = clube.IndexOf(" - ", StringComparison.Ordinal);
        if (idx < 0) idx = clube.IndexOf('-');
        string base_ = idx > 0 ? clube[..idx].Trim() : clube;
        return base_.Length <= 30 ? base_ : base_[..30];
    }

    private static string? CellStr(IRow row, int col)
    {
        var cell = row.GetCell(col);
        if (cell == null) return null;
        return cell.CellType switch
        {
            CellType.Numeric => DataFormatter(cell),
            CellType.String  => cell.StringCellValue,
            CellType.Formula => cell.CachedFormulaResultType == CellType.Numeric
                                    ? DataFormatter(cell)
                                    : cell.StringCellValue,
            _ => cell.ToString(),
        };
    }

    private static string DataFormatter(ICell cell)
    {
        // Preserva o texto exibido no Excel (e.g. "0.1874" sem arredondamento)
        var fmt = new NPOI.SS.UserModel.DataFormatter();
        return fmt.FormatCellValue(cell);
    }

    private static int CellInt(IRow row, int col)
    {
        var cell = row.GetCell(col);
        if (cell == null) return 0;
        if (cell.CellType == CellType.Numeric) return (int)cell.NumericCellValue;
        return int.TryParse(cell.ToString(), out int v) ? v : 0;
    }
}
