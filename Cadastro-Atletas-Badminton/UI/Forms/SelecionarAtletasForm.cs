using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class SelecionarAtletasForm : Form
{
    // ── Estado ───────────────────────────────────────────────────────────────
    private readonly Torneio         _torneio;
    private readonly List<Atleta>    _atletas;
    private readonly List<Categoria> _categorias;
    private readonly HashSet<int>    _jaInscritos;
    private readonly int             _anoRef;
    private string                   _tipo = "SIMPLES";

    // ── Controles ────────────────────────────────────────────────────────────
    private readonly DataGridView _grid      = new();
    private readonly TextBox      _txtFiltro = new() { PlaceholderText = "Buscar por nome ou clube...", Width = 270, Height = 28 };
    private readonly Button       _btnSimples = TipoBotao("Simples");
    private readonly Button       _btnDupla   = TipoBotao("Dupla");
    private readonly Button       _btnMista   = TipoBotao("Mista");
    private readonly Label        _lblStatus  = new() { AutoSize = true };
    private readonly Label        _lblDica    = new() { AutoSize = true, ForeColor = Color.Gray };
    private readonly Button       _btnInscrever;

    public SelecionarAtletasForm(Torneio torneio)
    {
        _torneio    = torneio;
        _anoRef     = torneio.DataInicio?.Year ?? DateTime.Now.Year;
        _atletas    = AtletasService.ListarComEntidade().OrderBy(a => a.NomeCompleto).ToList();
        _categorias = CategoriasService.Listar().ToList();

        var inscritos = InscricoesService.ListarPorTorneio(torneio.Id).ToList();
        _jaInscritos  = inscritos.Select(i => i.Atleta1Id)
            .Concat(inscritos.Where(i => i.Atleta2Id.HasValue).Select(i => i.Atleta2Id!.Value))
            .ToHashSet();

        _btnInscrever = Estilos.CriarBotao("Inscrever", "novo");
        _btnInscrever.Width = 120;

        Text          = $"Adicionar Atletas — {torneio.Nome}";
        ClientSize    = new Size(800, 560);
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize   = new Size(640, 420);
        BackColor     = Estilos.ContentBg;

        ConfigurarGrid();
        MontarLayout();
        AplicarTipo("SIMPLES");
    }

    // ── Grid ─────────────────────────────────────────────────────────────────

    private void ConfigurarGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = true;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.AutoGenerateColumns = false;
        _grid.RowHeadersVisible = false;
        _grid.Cursor = Cursors.Hand;

        _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Sel",   HeaderText = "",          Width = 40, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, ReadOnly = false });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Nome",  HeaderText = "Nome",      FillWeight = 40, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Clube", HeaderText = "Clube",     FillWeight = 14, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Ano",   HeaderText = "Nasc.",     FillWeight = 9,  ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Faixa", HeaderText = "Faixa",     FillWeight = 13, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Cat",   HeaderText = "Categoria", FillWeight = 12, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn  { Name = "Obs",   HeaderText = "",          FillWeight = 12, ReadOnly = true });

        Estilos.EstilizarGrid(_grid);

        _grid.CellClick          += Grid_CellClick;
        _grid.KeyDown            += Grid_KeyDown;
        _grid.CellValueChanged   += (_, _) => AtualizarStatus();
        _grid.CurrentCellDirtyStateChanged += (_, _) => { if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };
    }

    // ── Layout ───────────────────────────────────────────────────────────────

    private void MontarLayout()
    {
        // Header
        var header = Estilos.CriarHeader($"Adicionar Atletas");

        // Barra de tipo + filtro
        var toolPanel = new Panel { Dock = DockStyle.Top, Height = 50, BackColor = Color.White, Padding = new Padding(12, 9, 12, 0) };
        toolPanel.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Estilos.BorderColor), 0, toolPanel.Height - 1, toolPanel.Width, toolPanel.Height - 1);

        var toolFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };

        _txtFiltro.Margin = new Padding(14, 0, 4, 0);
        var btnBuscar = new Button
        {
            Text = "Buscar", Width = 78, Height = 28, FlatStyle = FlatStyle.Flat,
            BackColor = Estilos.AccentBlue, ForeColor = Color.White, Cursor = Cursors.Hand,
        };
        btnBuscar.FlatAppearance.BorderSize = 0;
        btnBuscar.Click += (_, _) => CarregarGrid();
        _txtFiltro.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) CarregarGrid(); };

        toolFlow.Controls.AddRange(new Control[] { _btnSimples, _btnDupla, _btnMista, _txtFiltro, btnBuscar });
        toolPanel.Controls.Add(toolFlow);

        // Barra de status + botões
        var statusPanel = new Panel { Dock = DockStyle.Bottom, Height = 54, BackColor = Color.White };
        statusPanel.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Estilos.BorderColor), 0, 0, statusPanel.Width, 0);

        _lblStatus.Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9.5f, FontStyle.Bold);
        _lblStatus.ForeColor = Estilos.AccentBlue;
        _lblDica.Font        = new Font(SystemFonts.DefaultFont!.FontFamily, 8f);

        var infoFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 440, FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(12, 6, 0, 0), WrapContents = false,
        };
        infoFlow.Controls.AddRange(new Control[] { _lblStatus, _lblDica });

        var btnCancelar = Estilos.CriarBotao("Cancelar", "padrao");
        btnCancelar.DialogResult = DialogResult.Cancel;
        var btnFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, Width = 250, FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(0, 10, 12, 0), WrapContents = false,
        };
        btnFlow.Controls.AddRange(new Control[] { _btnInscrever, btnCancelar });

        statusPanel.Controls.Add(infoFlow);
        statusPanel.Controls.Add(btnFlow);

        // Montagem
        Controls.Add(_grid);
        Controls.Add(statusPanel);
        Controls.Add(toolPanel);
        Controls.Add(header);

        CancelButton = btnCancelar;
        _btnInscrever.Click += BtnInscrever_Click;
        _btnSimples.Click   += (_, _) => AplicarTipo("SIMPLES");
        _btnDupla.Click     += (_, _) => AplicarTipo("DUPLA");
        _btnMista.Click     += (_, _) => AplicarTipo("MISTA");
    }

    // ── Tipo de modalidade ───────────────────────────────────────────────────

    private void AplicarTipo(string tipo)
    {
        _tipo = tipo;
        foreach (var (btn, t) in new[] { (_btnSimples, "SIMPLES"), (_btnDupla, "DUPLA"), (_btnMista, "MISTA") })
        {
            btn.BackColor = t == tipo ? Estilos.AccentBlue : Color.FromArgb(220, 225, 232);
            btn.ForeColor = t == tipo ? Color.White : Estilos.TextPrimary;
        }
        CarregarGrid();
    }

    // ── Dados ────────────────────────────────────────────────────────────────

    private void CarregarGrid()
    {
        string filtro = _txtFiltro.Text.Trim().ToLowerInvariant();

        // Preserva seleções
        var selecionados = _grid.Rows.Cast<DataGridViewRow>()
            .Where(r => r.Cells["Sel"].Value is true && r.Tag is Atleta)
            .Select(r => ((Atleta)r.Tag!).Id)
            .ToHashSet();

        _grid.Rows.Clear();

        foreach (var atleta in _atletas)
        {
            if (!string.IsNullOrEmpty(filtro))
            {
                bool nomeOk  = atleta.NomeCompleto.ToLowerInvariant().Contains(filtro);
                bool clubeOk = (atleta.Entidade?.Sigla ?? "").ToLowerInvariant().Contains(filtro);
                if (!nomeOk && !clubeOk) continue;
            }

            string faixa  = FaixaLabel(atleta.AnoNascimento);
            string codigo = CodigoCategoria(atleta);
            string catTxt = _categorias.FirstOrDefault(c => c.Codigo == codigo)?.Codigo ?? $"{codigo}?";
            bool jaInscrito = _jaInscritos.Contains(atleta.Id);

            int idx = _grid.Rows.Add();
            var row = _grid.Rows[idx];
            row.Tag = atleta;
            row.Cells["Sel"].Value   = selecionados.Contains(atleta.Id);
            row.Cells["Nome"].Value  = atleta.NomeCompleto;
            row.Cells["Clube"].Value = atleta.Entidade?.Sigla ?? "—";
            row.Cells["Ano"].Value   = atleta.AnoNascimento;
            row.Cells["Faixa"].Value = faixa;
            row.Cells["Cat"].Value   = catTxt;
            row.Cells["Obs"].Value   = jaInscrito ? "já inscrito" : "";

            if (jaInscrito)
                row.DefaultCellStyle.ForeColor = Color.Silver;
        }

        AtualizarStatus();
    }

    // ── Interação com o grid ─────────────────────────────────────────────────

    private void Grid_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        var row = _grid.Rows[e.RowIndex];

        // Clique em qualquer célula da linha = toggle checkbox
        bool atual = row.Cells["Sel"].Value is true;
        row.Cells["Sel"].Value = !atual;
        _grid.InvalidateRow(e.RowIndex);
        AtualizarStatus();
    }

    private void Grid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space && _grid.CurrentRow != null)
        {
            var row = _grid.CurrentRow;
            row.Cells["Sel"].Value = !(row.Cells["Sel"].Value is true);
            AtualizarStatus();
            e.Handled = true;
        }
        else if (e.KeyCode == Keys.A && e.Control)
        {
            // Ctrl+A = marcar todos
            bool algumDesmarcado = _grid.Rows.Cast<DataGridViewRow>().Any(r => !(r.Cells["Sel"].Value is true));
            foreach (DataGridViewRow row in _grid.Rows)
                row.Cells["Sel"].Value = algumDesmarcado;
            AtualizarStatus();
            e.Handled = true;
        }
    }

    // ── Status ───────────────────────────────────────────────────────────────

    private void AtualizarStatus()
    {
        int n = _grid.Rows.Cast<DataGridViewRow>().Count(r => r.Cells["Sel"].Value is true);

        if (_tipo == "SIMPLES")
        {
            _lblStatus.Text = n == 0 ? "Nenhum atleta selecionado"
                                     : $"{n} atleta(s) selecionado(s)";
            _lblDica.Text   = "Cada atleta gera uma inscrição com categoria detectada automaticamente.";
        }
        else
        {
            int pares    = n / 2;
            int restante = n % 2;
            _lblStatus.Text = n == 0 ? "Nenhum atleta selecionado"
                            : $"{n} selecionado(s) → {pares} dupla(s)" + (restante > 0 ? " + 1 aguardando par" : "");
            _lblDica.Text   = "Pareados em ordem de seleção: 1°+2° = Par 1, 3°+4° = Par 2 ...  (Ctrl+A = todos)";
        }

        _btnInscrever.Enabled = n > 0;
    }

    // ── Inscrever ────────────────────────────────────────────────────────────

    private void BtnInscrever_Click(object? sender, EventArgs e)
    {
        var selecionados = _grid.Rows.Cast<DataGridViewRow>()
            .Where(r => r.Cells["Sel"].Value is true && r.Tag is Atleta)
            .Select(r => (Atleta)r.Tag!)
            .ToList();

        if (selecionados.Count == 0)
        {
            MessageBox.Show("Selecione pelo menos um atleta.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_tipo != "SIMPLES" && selecionados.Count % 2 != 0)
        {
            MessageBox.Show($"Para {_tipo}, selecione um número par de atletas ({selecionados.Count} selecionado(s)).",
                "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int ok = 0, erros = 0;
        var semCategoria = new List<string>();

        if (_tipo == "SIMPLES")
        {
            foreach (var a in selecionados)
            {
                var cat = _categorias.FirstOrDefault(c => c.Codigo == CodigoCategoria(a));
                if (cat == null) { semCategoria.Add(a.NomeCompleto); erros++; continue; }
                try { InscricoesService.Inserir(new Inscricao { TorneioId = _torneio.Id, Atleta1Id = a.Id, CategoriaId = cat.Id }); ok++; }
                catch { erros++; }
            }
        }
        else
        {
            for (int i = 0; i < selecionados.Count - 1; i += 2)
            {
                var a1 = selecionados[i];
                var a2 = selecionados[i + 1];
                var cat = _categorias.FirstOrDefault(c => c.Codigo == CodigoCategoria(a1));
                if (cat == null) { semCategoria.Add($"{a1.NomeCompleto} / {a2.NomeCompleto}"); erros++; continue; }
                try { InscricoesService.Inserir(new Inscricao { TorneioId = _torneio.Id, Atleta1Id = a1.Id, Atleta2Id = a2.Id, CategoriaId = cat.Id }); ok++; }
                catch { erros++; }
            }
        }

        string msg = $"{ok} inscrição(ões) criada(s) com sucesso!";
        if (semCategoria.Count > 0)
            msg += $"\n\nSem categoria detectada (ajuste via Editar):\n• " + string.Join("\n• ", semCategoria);
        if (erros > 0 && semCategoria.Count == 0)
            msg += $"\n\n{erros} erro(s) (atleta já inscrito na mesma categoria?).";

        MessageBox.Show(msg, "Inscrições criadas", MessageBoxButtons.OK,
            erros > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);

        DialogResult = DialogResult.OK;
        Close();
    }

    // ── Cálculo de faixa/categoria ───────────────────────────────────────────

    private string CodigoCategoria(Atleta a)
    {
        string faixa = a.AnoNascimento switch
        {
            var n when n >= _anoRef - 8  => "09",
            var n when n >= _anoRef - 10 => "11",
            var n when n >= _anoRef - 12 => "13",
            var n when n >= _anoRef - 14 => "15",
            var n when n >= _anoRef - 16 => "17",
            var n when n >= _anoRef - 18 => "19",
            var n when n > _anoRef - 35  => "A",
            _                            => "35"
        };
        string prefixo = _tipo switch
        {
            "SIMPLES" => a.Sexo == "M" ? "SM" : "SF",
            "DUPLA"   => a.Sexo == "M" ? "DM" : "DF",
            _         => "DX",
        };
        return prefixo + faixa;
    }

    private string FaixaLabel(int ano) => ano switch
    {
        var n when n >= _anoRef - 8  => "Sub-9",
        var n when n >= _anoRef - 10 => "Sub-11",
        var n when n >= _anoRef - 12 => "Sub-13",
        var n when n >= _anoRef - 14 => "Sub-15",
        var n when n >= _anoRef - 16 => "Sub-17",
        var n when n >= _anoRef - 18 => "Sub-19",
        var n when n > _anoRef - 35  => "Adulto",
        _                            => "+35"
    };

    private static Button TipoBotao(string texto)
    {
        var btn = new Button
        {
            Text      = texto,
            Width     = 86,
            Height    = 30,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(220, 225, 232),
            ForeColor = Estilos.TextPrimary,
            Cursor    = Cursors.Hand,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f),
            Margin    = new Padding(0, 0, 5, 0),
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }
}
