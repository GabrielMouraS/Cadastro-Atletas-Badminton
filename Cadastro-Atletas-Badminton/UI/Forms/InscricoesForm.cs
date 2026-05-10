using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class InscricoesForm : Form
{
    private readonly Torneio      _torneio;
    private readonly DataGridView _grid = new();

    public InscricoesForm(Torneio torneio)
    {
        _torneio = torneio;
        Text = $"Inscrições — {torneio.Nome}";
        ClientSize = new System.Drawing.Size(1050, 520);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new System.Drawing.Size(800, 380);

        ConfigurarGrid();

        var btnNovo    = new Button { Text = "Nova",    Width = 90 };
        var btnEditar  = new Button { Text = "Editar",  Width = 90 };
        var btnExcluir = new Button { Text = "Excluir", Width = 90 };

        btnNovo.Click    += BtnNovo_Click;
        btnEditar.Click  += BtnEditar_Click;
        btnExcluir.Click += BtnExcluir_Click;
        _grid.CellDoubleClick += (_, _) => BtnEditar_Click(null, EventArgs.Empty);

        var lblInfo = new Label
        {
            Text = $"Torneio: {torneio.Nome}  |  Tipo: {torneio.TipoFicha}",
            Dock = DockStyle.Top,
            Height = 28,
            Padding = new Padding(6, 6, 0, 0),
            Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont!.FontFamily, 9f, System.Drawing.FontStyle.Bold)
        };

        var btnPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 44,
            Padding = new Padding(6),
            FlowDirection = FlowDirection.LeftToRight
        };
        btnPanel.Controls.AddRange(new Control[] { btnNovo, btnEditar, btnExcluir });

        Controls.Add(_grid);
        Controls.Add(lblInfo);
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

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Categoria",  HeaderText = "Categoria",        FillWeight = 12 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Atleta1",    HeaderText = "Atleta 1",         FillWeight = 25 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Atleta2",    HeaderText = "Atleta 2",         FillWeight = 25 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RkInterno",  HeaderText = "RK Interno",       FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RkEstadual", HeaderText = "RK Estadual",      FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Remanej",   HeaderText = "Remanej.",         FillWeight = 8  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Valor",      HeaderText = "Valor (R$)",       FillWeight = 10 });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Pago",      HeaderText = "Pago",             FillWeight = 7  });
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
            row.Cells["RkInterno"].Value  = i.RkInterno?.ToString() ?? "-";
            row.Cells["RkEstadual"].Value = i.RkEstadual?.ToString() ?? "-";
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
