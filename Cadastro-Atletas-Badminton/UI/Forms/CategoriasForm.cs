using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class CategoriasForm : Form
{
    private readonly DataGridView _grid = new();

    public CategoriasForm()
    {
        Text      = "Categorias";
        BackColor = Estilos.ContentBg;

        ConfigurarGrid();

        var btnNovo    = Estilos.CriarBotao("Novo",    "novo");
        var btnEditar  = Estilos.CriarBotao("Editar",  "editar");
        var btnExcluir = Estilos.CriarBotao("Excluir", "excluir");

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var header    = Estilos.CriarHeader("Categorias");
        var barraBtns = Estilos.CriarBarraBotoes(btnNovo, btnEditar, btnExcluir);

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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo",        HeaderText = "Código",        FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descricao",     HeaderText = "Descrição",     FillWeight = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tipo",          HeaderText = "Tipo",          FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ChavePlanilha", HeaderText = "Chave",         FillWeight = 15 });

        Estilos.EstilizarGrid(_grid);
    }

    private void CarregarDados() =>
        _grid.DataSource = CategoriasService.Listar().ToList();

    private Categoria? Selecionada() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].DataBoundItem as Categoria : null;

    private void BtnNovo_Click(object? s, EventArgs e)
    {
        using var dlg = new CategoriaEditForm();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { CategoriasService.Inserir(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var cat = Selecionada();
        if (cat == null) { MostrarAviso("Selecione uma categoria."); return; }

        using var dlg = new CategoriaEditForm(cat);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { CategoriasService.Atualizar(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object? s, EventArgs e)
    {
        var cat = Selecionada();
        if (cat == null) { MostrarAviso("Selecione uma categoria."); return; }
        if (!ConfirmarExclusao(cat.Codigo)) return;
        try   { CategoriasService.Excluir(cat.Id); CarregarDados(); }
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
