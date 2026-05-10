using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class AtletasForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly TextBox    _txtFiltroNome    = new() { Width = 200, PlaceholderText = "Nome..." };
    private readonly ComboBox   _cmbFiltroSexo    = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };
    private readonly ComboBox   _cmbFiltroEntidade = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };

    public AtletasForm()
    {
        Text = "Atletas";
        ClientSize = new System.Drawing.Size(920, 520);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new System.Drawing.Size(700, 380);

        ConfigurarGrid();
        ConfigurarFiltros();

        var btnNovo    = new Button { Text = "Novo",    Width = 90 };
        var btnEditar  = new Button { Text = "Editar",  Width = 90 };
        var btnExcluir = new Button { Text = "Excluir", Width = 90 };

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        // Filtros
        var filtroPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(6, 6, 6, 0),
            FlowDirection = FlowDirection.LeftToRight
        };
        filtroPanel.Controls.Add(new Label { Text = "Nome:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        filtroPanel.Controls.Add(_txtFiltroNome);
        filtroPanel.Controls.Add(new Label { Text = "Sexo:", AutoSize = true, Padding = new Padding(6, 6, 0, 0) });
        filtroPanel.Controls.Add(_cmbFiltroSexo);
        filtroPanel.Controls.Add(new Label { Text = "Entidade:", AutoSize = true, Padding = new Padding(6, 6, 0, 0) });
        filtroPanel.Controls.Add(_cmbFiltroEntidade);
        var btnFiltrar = new Button { Text = "Filtrar", Width = 75 };
        btnFiltrar.Click += (_, _) => CarregarDados();
        filtroPanel.Controls.Add(btnFiltrar);

        // Botões
        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            Padding = new Padding(6),
            FlowDirection = FlowDirection.LeftToRight
        };
        btnPanel.Controls.AddRange(new Control[] { btnNovo, btnEditar, btnExcluir });

        Controls.Add(_grid);
        Controls.Add(filtroPanel);
        Controls.Add(btnPanel);

        CarregarDados();
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
        _grid.BackgroundColor = System.Drawing.SystemColors.Window;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeCompleto",    HeaderText = "Nome Completo",      FillWeight = 35 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "AnoNascimento",   HeaderText = "Ano Nasc.",          FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sexo",            HeaderText = "Sexo",               FillWeight = 8  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CodigoFederacao", HeaderText = "Cód. Federação",     FillWeight = 18 });

        // Coluna derivada de Entidade (objeto aninhado)
        var colEntidade = new DataGridViewTextBoxColumn { HeaderText = "Entidade", FillWeight = 27 };
        colEntidade.DataPropertyName = "Entidade";
        _grid.Columns.Add(colEntidade);
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

        // Coluna Entidade: exibir sigla do objeto aninhado
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.DataBoundItem is Atleta a)
                row.Cells["Entidade"].Value = a.Entidade?.Sigla ?? "-";
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
