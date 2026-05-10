using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class InscricoesForm : Form
{
    private readonly Torneio      _torneio;
    private readonly DataGridView _grid = new();

    public InscricoesForm(Torneio torneio)
    {
        _torneio = torneio;
        Text          = $"Inscrições — {torneio.Nome}";
        ClientSize    = new System.Drawing.Size(1050, 540);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize   = new System.Drawing.Size(800, 400);
        BackColor     = Estilos.ContentBg;

        ConfigurarGrid();

        var btnNovo    = Estilos.CriarBotao("Nova",    "novo");
        var btnEditar  = Estilos.CriarBotao("Editar",  "editar");
        var btnExcluir = Estilos.CriarBotao("Excluir", "excluir");

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var header    = Estilos.CriarHeader("Inscrições", torneio.Nome);
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Categoria",  HeaderText = "Categoria",   FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Atleta1",    HeaderText = "Atleta 1",    FillWeight = 25 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Atleta2",    HeaderText = "Atleta 2",    FillWeight = 25 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "RkInterno",  HeaderText = "RK Interno",  FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "RkEstadual", HeaderText = "RK Estadual", FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Remanej",    HeaderText = "Remanej.",    FillWeight = 8  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Valor",      HeaderText = "Valor (R$)",  FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Pago",       HeaderText = "Pago",        FillWeight = 7  });

        Estilos.EstilizarGrid(_grid);
    }

    private void CarregarDados()
    {
        var inscricoes = InscricoesService.ListarPorTorneio(_torneio.Id).ToList();
        _grid.Rows.Clear();

        foreach (var i in inscricoes)
        {
            int idx = _grid.Rows.Add();
            var row = _grid.Rows[idx];
            row.Tag = i;
            row.Cells["Categoria"].Value  = i.Categoria?.Codigo ?? i.CategoriaId.ToString();
            row.Cells["Atleta1"].Value    = i.Atleta1?.NomeCompleto ?? i.Atleta1Id.ToString();
            row.Cells["Atleta2"].Value    = i.Atleta2?.NomeCompleto ?? string.Empty;
            row.Cells["RkInterno"].Value  = i.RkInterno?.ToString() ?? "—";
            row.Cells["RkEstadual"].Value = i.RkEstadual?.ToString() ?? "—";
            row.Cells["Remanej"].Value    = i.AceitaRemanejamento;
            row.Cells["Valor"].Value      = i.Valor.ToString("F2");
            row.Cells["Pago"].Value       = i.Pago;
        }
    }

    private Inscricao? Selecionada() =>
        _grid.SelectedRows.Count > 0 ? _grid.SelectedRows[0].Tag as Inscricao : null;

    private void BtnNovo_Click(object? s, EventArgs e)
    {
        using var dlg = new InscricaoEditForm(_torneio);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { InscricoesService.Inserir(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnEditar_Click(object? s, EventArgs e)
    {
        var insc = Selecionada();
        if (insc == null) { MostrarAviso("Selecione uma inscrição."); return; }

        using var dlg = new InscricaoEditForm(_torneio, insc);
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { InscricoesService.Atualizar(dlg.Resultado); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private void BtnExcluir_Click(object? s, EventArgs e)
    {
        var insc = Selecionada();
        if (insc == null) { MostrarAviso("Selecione uma inscrição."); return; }

        string label = insc.Atleta1?.NomeCompleto ?? insc.Atleta1Id.ToString();
        if (!ConfirmarExclusao(label)) return;
        try   { InscricoesService.Excluir(insc.Id); CarregarDados(); }
        catch (Exception ex) { MostrarErro(ex); }
    }

    private static void MostrarErro(Exception ex) =>
        MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static void MostrarAviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    private static bool ConfirmarExclusao(string nome) =>
        MessageBox.Show($"Excluir inscrição de \"{nome}\"?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}
