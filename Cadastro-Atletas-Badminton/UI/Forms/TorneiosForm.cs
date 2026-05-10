using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class TorneiosForm : Form
{
    private readonly DataGridView _grid = new();

    public TorneiosForm()
    {
        Text = "Torneios";
        ClientSize = new System.Drawing.Size(860, 480);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new System.Drawing.Size(650, 350);

        ConfigurarGrid();

        var btnNovo       = new Button { Text = "Novo",        Width = 90 };
        var btnEditar     = new Button { Text = "Editar",      Width = 90 };
        var btnExcluir    = new Button { Text = "Excluir",     Width = 90 };
        var btnInscricoes = new Button { Text = "Inscrições…", Width = 110 };

        btnNovo.Click       += BtnNovo_Click;
        btnEditar.Click     += BtnEditar_Click;
        btnExcluir.Click    += BtnExcluir_Click;
        btnInscricoes.Click += BtnInscricoes_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            Padding = new Padding(6),
            FlowDirection = FlowDirection.LeftToRight
        };
        btnPanel.Controls.AddRange(new Control[] { btnNovo, btnEditar, btnExcluir, btnInscricoes });

        Controls.Add(_grid);
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nome",            HeaderText = "Nome",           FillWeight = 35 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TipoFicha",       HeaderText = "Tipo",           FillWeight = 18 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DataInicio",      HeaderText = "Data Início",    FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Local",           HeaderText = "Local",          FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ResponsavelNome", HeaderText = "Responsável",    FillWeight = 12 });
    }

    private void CarregarDados() =>
        _grid.DataSource = TorneiosService.Listar().ToList();

    private Torneio? Selecionado() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as Torneio : null;

    private void BtnNovo_Click(object? s, EventArgs e)
    {
        using var dlg = new TorneioEditForm();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { TorneiosService.Inserir(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var torneio = Selecionado();
        if (torneio == null) { MostrarAviso("Selecione um torneio."); return; }

        using var dlg = new TorneioEditForm(torneio);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { TorneiosService.Atualizar(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object? s, EventArgs e)
    {
        var torneio = Selecionado();
        if (torneio == null) { MostrarAviso("Selecione um torneio."); return; }
        if (!ConfirmarExclusao(torneio.Nome)) return;
        try   { TorneiosService.Excluir(torneio.Id); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnInscricoes_Click(object? s, EventArgs e)
    {
        var torneio = Selecionado();
        if (torneio == null) { MostrarAviso("Selecione um torneio."); return; }
        new InscricoesForm(torneio).ShowDialog(this);
    }

    private static void MostrarErro(Exception ex) =>
        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void MostrarAviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static bool ConfirmarExclusao(string nome) =>
        MessageBox.Show($"Excluir torneio \"{nome}\" e TODAS as suas inscrições?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
