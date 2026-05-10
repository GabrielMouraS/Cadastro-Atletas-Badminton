using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class AtletasForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly TextBox  _txtFiltroNome     = new() { Width = 200, PlaceholderText = "Nome..." };
    private readonly ComboBox _cmbFiltroSexo     = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
    private readonly ComboBox _cmbFiltroEntidade = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };

    public AtletasForm()
    {
        Text      = "Atletas";
        BackColor = Estilos.ContentBg;

        ConfigurarGrid();
        ConfigurarFiltros();

        var btnNovo    = Estilos.CriarBotao("Novo",    "novo");
        var btnEditar  = Estilos.CriarBotao("Editar",  "editar");
        var btnExcluir = Estilos.CriarBotao("Excluir", "excluir");

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var header    = Estilos.CriarHeader("Atletas");
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

        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents  = false,
        };

        flow.Controls.Add(RotuloFiltro("Nome:"));
        flow.Controls.Add(_txtFiltroNome);
        flow.Controls.Add(RotuloFiltro("Sexo:"));
        flow.Controls.Add(_cmbFiltroSexo);
        flow.Controls.Add(RotuloFiltro("Entidade:"));
        flow.Controls.Add(_cmbFiltroEntidade);

        var btnFiltrar = new Button
        {
            Text      = "Filtrar",
            Width     = 80,
            Height    = 28,
            FlatStyle = FlatStyle.Flat,
            BackColor = Estilos.AccentBlue,
            ForeColor = Color.White,
            Cursor    = Cursors.Hand,
            Margin    = new Padding(8, 0, 0, 0),
        };
        btnFiltrar.FlatAppearance.BorderSize = 0;
        btnFiltrar.Click += (_, _) => CarregarDados();
        flow.Controls.Add(btnFiltrar);

        _txtFiltroNome.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) CarregarDados(); };

        panel.Controls.Add(flow);
        return panel;
    }

    private static Label RotuloFiltro(string texto) => new()
    {
        Text      = texto,
        AutoSize  = false,
        Width     = texto.Length > 6 ? 70 : 42,
        Height    = 28,
        TextAlign = ContentAlignment.MiddleLeft,
        Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f),
        ForeColor = Estilos.TextPrimary,
        Margin    = new Padding(6, 0, 2, 0),
    };

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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeCompleto",    HeaderText = "Nome Completo",  FillWeight = 35 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AnoNascimento",   HeaderText = "Ano Nasc.",      FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sexo",            HeaderText = "Sexo",           FillWeight = 8  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CodigoFederacao", HeaderText = "Cód. Federação", FillWeight = 18 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Entidade",                    HeaderText = "Entidade",       FillWeight = 27 });

        Estilos.EstilizarGrid(_grid);
    }

    private void ConfigurarFiltros()
    {
        _cmbFiltroSexo.Items.Add("Todos");
        _cmbFiltroSexo.Items.Add("M");
        _cmbFiltroSexo.Items.Add("F");
        _cmbFiltroSexo.SelectedIndex = 0;

        var entidades = EntidadesService.Listar().ToList();
        entidades.Insert(0, new Entidade { Id = 0, Sigla = "Todas" });
        _cmbFiltroEntidade.DataSource    = entidades;
        _cmbFiltroEntidade.DisplayMember = "Sigla";
        _cmbFiltroEntidade.ValueMember   = "Id";
        _cmbFiltroEntidade.SelectedIndex = 0;
    }

    private void CarregarDados()
    {
        string? nome      = string.IsNullOrWhiteSpace(_txtFiltroNome.Text) ? null : _txtFiltroNome.Text.Trim();
        string? sexo      = _cmbFiltroSexo.SelectedIndex <= 0 ? null : (string)_cmbFiltroSexo.SelectedItem!;
        int?    entidadeId = _cmbFiltroEntidade.SelectedValue is int id && id > 0 ? id : null;

        var atletas = AtletasService.ListarComEntidade(nome, sexo, entidadeId).ToList();
        _grid.DataSource = atletas;

        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is Atleta a)
                row.Cells["Entidade"].Value = a.Entidade?.Sigla ?? "—";
        }
    }

    private Atleta? Selecionado() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as Atleta : null;

    private void BtnNovo_Click(object? s, EventArgs e)
    {
        using var dlg = new AtletaEditForm();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { AtletasService.Inserir(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var atleta = Selecionado();
        if (atleta == null) { MostrarAviso("Selecione um atleta."); return; }

        using var dlg = new AtletaEditForm(atleta);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { AtletasService.Atualizar(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object? s, EventArgs e)
    {
        var atleta = Selecionado();
        if (atleta == null) { MostrarAviso("Selecione um atleta."); return; }
        if (!ConfirmarExclusao(atleta.NomeCompleto)) return;
        try   { AtletasService.Excluir(atleta.Id); CarregarDados(); }
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
