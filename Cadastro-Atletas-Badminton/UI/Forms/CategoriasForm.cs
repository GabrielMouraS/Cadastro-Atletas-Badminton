using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class CategoriasForm : Form
{
    private readonly DataGridView _grid = new();

    public CategoriasForm()
    {
        Text = "Categorias";
        ClientSize = new System.Drawing.Size(720, 480);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new System.Drawing.Size(600, 350);

        ConfigurarGrid();

        var btnNovo    = new Button { Text = "Novo",    Width = 90 };
        var btnEditar  = new Button { Text = "Editar",  Width = 90 };
        var btnExcluir = new Button { Text = "Excluir", Width = 90 };

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            Padding = new Padding(6),
            FlowDirection = FlowDirection.LeftToRight
        };
        btnPanel.Controls.AddRange(new Control[] { btnNovo, btnEditar, btnExcluir });

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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo",        HeaderText = "Código",          FillWeight = 15 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Descricao",     HeaderText = "Descrição",        FillWeight = 50 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Tipo",          HeaderText = "Tipo",             FillWeight = 20 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ChavePlanilha", HeaderText = "Chave Planilha",   FillWeight = 15 });
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
