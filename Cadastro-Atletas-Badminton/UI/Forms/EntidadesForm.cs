using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class EntidadesForm : Form
{
    private readonly DataGridView _grid       = new();
    private readonly TextBox      _txtFiltro  = new() { Width = 260, PlaceholderText = "Buscar por sigla, nome ou cidade..." };
    private readonly Label        _lblContador = new() { AutoSize = true, ForeColor = Estilos.AccentBlue };
    private List<Entidade>        _todas      = [];

    public EntidadesForm()
    {
        Text      = "Entidades";
        BackColor = Estilos.ContentBg;

        ConfigurarGrid();

        var btnNovo    = Estilos.CriarBotao("Novo",    "novo");
        var btnEditar  = Estilos.CriarBotao("Editar",  "editar");
        var btnExcluir = Estilos.CriarBotao("Excluir", "excluir");

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        // Filtro ao digitar — sem botão, resposta imediata
        _txtFiltro.TextChanged += (_, _) => AplicarFiltro();

        var header    = Estilos.CriarHeader("Entidades");
        var barraBtns = Estilos.CriarBarraBotoes(btnNovo, btnEditar, btnExcluir);
        var filtroPanel = CriarFiltroPanel();

        Controls.Add(_grid);
        Controls.Add(barraBtns);
        Controls.Add(filtroPanel);
        Controls.Add(header);

        CarregarDados();
    }

    private Panel CriarFiltroPanel()
    {
        var panel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 44,
            BackColor = Color.White,
            Padding   = new Padding(12, 6, 12, 6),
        };
        panel.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(Estilos.BorderColor), 0, panel.Height - 1, panel.Width, panel.Height - 1);

        var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        var lbl  = new Label
        {
            Text      = "Buscar:",
            AutoSize  = false,
            Width     = 52,
            Height    = 28,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f),
            ForeColor = Estilos.TextPrimary,
        };
        _lblContador.Font   = new Font(SystemFonts.DefaultFont!.FontFamily, 9f, FontStyle.Bold);
        _lblContador.Margin = new Padding(16, 5, 0, 0);
        flow.Controls.Add(lbl);
        flow.Controls.Add(_txtFiltro);
        flow.Controls.Add(_lblContador);
        panel.Controls.Add(flow);
        return panel;
    }

    private void ConfigurarGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sigla",        HeaderText = "Sigla",         FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeCompleto", HeaderText = "Nome Completo", FillWeight = 55 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cidade",       HeaderText = "Cidade",        FillWeight = 30 });

        Estilos.EstilizarGrid(_grid);
    }

    private void CarregarDados()
    {
        _todas = EntidadesService.Listar().ToList();
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        string q = _txtFiltro.Text.Trim().ToLowerInvariant();
        List<Entidade> resultado = string.IsNullOrEmpty(q)
            ? _todas
            : _todas.Where(e =>
                e.Sigla.ToLowerInvariant().Contains(q) ||
                e.NomeCompleto.ToLowerInvariant().Contains(q) ||
                (e.Cidade ?? "").ToLowerInvariant().Contains(q))
              .ToList();

        _grid.DataSource = resultado;

        int total = _todas.Count;
        int exibindo = resultado.Count;
        _lblContador.Text = string.IsNullOrEmpty(q)
            ? $"{total} entidade(s)"
            : $"{exibindo} de {total}";
        _lblContador.ForeColor = string.IsNullOrEmpty(q) ? Estilos.AccentBlue : Estilos.AccentOrange;
    }

    private Entidade? Selecionada() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as Entidade : null;

    private void BtnNovo_Click(object? s, EventArgs e)
    {
        using var dlg = new EntidadeEditForm();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { EntidadesService.Inserir(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var entidade = Selecionada();
        if (entidade == null) { MostrarAviso("Selecione uma entidade."); return; }

        using var dlg = new EntidadeEditForm(entidade);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { EntidadesService.Atualizar(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object? s, EventArgs e)
    {
        var entidade = Selecionada();
        if (entidade == null) { MostrarAviso("Selecione uma entidade."); return; }
        if (!ConfirmarExclusao(entidade.Sigla)) return;
        try   { EntidadesService.Excluir(entidade.Id); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private static void MostrarErro(Exception ex) =>
        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void MostrarAviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static bool ConfirmarExclusao(string nome) =>
        MessageBox.Show($"Excluir \"{nome}\"?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
