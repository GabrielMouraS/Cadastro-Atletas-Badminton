using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class TorneiosForm : Form
{
    private readonly DataGridView _grid = new();

    public TorneiosForm()
    {
        Text      = "Torneios";
        BackColor = Estilos.ContentBg;

        ConfigurarGrid();

        var btnNovo       = Estilos.CriarBotao("Novo",         "novo");
        var btnEditar     = Estilos.CriarBotao("Editar",       "editar");
        var btnExcluir    = Estilos.CriarBotao("Excluir",      "excluir");
        var btnInscricoes = Estilos.CriarBotao("Inscrições…",  "inscricoes");
        var btnExportar   = Estilos.CriarBotao("Exportar…",    "exportar");

        btnNovo.Click       += BtnNovo_Click;
        btnEditar.Click     += BtnEditar_Click;
        btnExcluir.Click    += BtnExcluir_Click;
        btnInscricoes.Click += BtnInscricoes_Click;
        btnExportar.Click   += BtnExportar_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var header    = Estilos.CriarHeader("Torneios");
        var barraBtns = Estilos.CriarBarraBotoes(btnNovo, btnEditar, btnExcluir, btnInscricoes, btnExportar);

        Controls.Add(_grid);
        Controls.Add(barraBtns);
        Controls.Add(header);

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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nome",            HeaderText = "Nome",        FillWeight = 35 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TipoFicha",       HeaderText = "Tipo",        FillWeight = 18 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DataInicio",      HeaderText = "Data Início", FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Local",           HeaderText = "Local",       FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ResponsavelNome", HeaderText = "Responsável", FillWeight = 12 });

        Estilos.EstilizarGrid(_grid);
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

    private void BtnExportar_Click(object? s, EventArgs e)
    {
        var torneio = Selecionado();
        if (torneio == null) { MostrarAviso("Selecione um torneio."); return; }
        new ExportarFichaForm(torneio).ShowDialog(this);
    }

    private static void MostrarErro(Exception ex) =>
        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void MostrarAviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static bool ConfirmarExclusao(string nome) =>
        MessageBox.Show($"Excluir torneio \"{nome}\" e TODAS as suas inscrições?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
