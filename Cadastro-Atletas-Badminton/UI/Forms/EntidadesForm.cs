using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class EntidadesForm : Form
{
    private readonly DataGridView _grid = new();

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

        var header   = Estilos.CriarHeader("Entidades");
        var barraBtns = Estilos.CriarBarraBotoes(btnNovo, btnEditar, btnExcluir);

        // Ordem de adição: Fill primeiro, depois os DockStyle.Bottom e DockStyle.Top
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Sigla",        HeaderText = "Sigla",         FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NomeCompleto", HeaderText = "Nome Completo", FillWeight = 55 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Cidade",       HeaderText = "Cidade",        FillWeight = 30 });

        Estilos.EstilizarGrid(_grid);
    }

    private void CarregarDados() =>
        _grid.DataSource = EntidadesService.Listar().ToList();

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
