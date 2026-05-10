using BadmintonCadastro.Models;
using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class InscricaoEditForm : Form
{
    // ── Resultado ────────────────────────────────────────────────────────────
    public Inscricao Resultado { get; } = new();

    // ── Estado ───────────────────────────────────────────────────────────────
    private string _tipo = "SIMPLES";
    private readonly int             _anoRef;
    private readonly List<Atleta>    _atletas;
    private readonly List<Categoria> _categorias;
    private List<Categoria>          _categoriasDoTipo = [];

    // ── Seletor de tipo ──────────────────────────────────────────────────────
    private readonly Button _btnSimples = TipoBotao("Simples");
    private readonly Button _btnDupla   = TipoBotao("Dupla");
    private readonly Button _btnMista   = TipoBotao("Mista");

    // ── Atletas ──────────────────────────────────────────────────────────────
    private readonly ComboBox _cmbAtleta1 = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbAtleta2 = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label    _lblFaixa1  = InfoLabel();
    private readonly Label    _lblFaixa2  = InfoLabel();
    private readonly Label    _lblAtleta2 = RotuloLabel("Atleta 2:");

    // ── Categoria ────────────────────────────────────────────────────────────
    private readonly ComboBox _cmbCategoria = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    // ── Outros campos ────────────────────────────────────────────────────────
    private readonly NumericUpDown _nudRkInterno  = new() { Minimum = 0, Maximum = 9999, Value = 0 };
    private readonly NumericUpDown _nudRkEstadual = new() { Minimum = 0, Maximum = 9999, Value = 0 };
    private readonly CheckBox      _chkRemanej    = new() { Text = "Aceita remanejamento", AutoSize = true };
    private readonly TextBox       _txtObsRemanej = new() { MaxLength = 500, PlaceholderText = "Obs. remanejamento..." };
    private readonly NumericUpDown _nudValor      = new() { DecimalPlaces = 2, Minimum = 0, Maximum = 99999, Increment = 1, Width = 100 };
    private readonly CheckBox      _chkPago       = new() { Text = "Pago", AutoSize = true };

    // ── Constructor ──────────────────────────────────────────────────────────
    public InscricaoEditForm(Torneio torneio, Inscricao? existente = null)
    {
        Resultado.TorneioId = torneio.Id;
        _anoRef = torneio.DataInicio?.Year ?? DateTime.Now.Year;

        if (existente != null)
        {
            Resultado.Id                  = existente.Id;
            Resultado.TorneioId           = existente.TorneioId;
            Resultado.CategoriaId         = existente.CategoriaId;
            Resultado.Atleta1Id           = existente.Atleta1Id;
            Resultado.Atleta2Id           = existente.Atleta2Id;
            Resultado.RkInterno           = existente.RkInterno;
            Resultado.RkEstadual          = existente.RkEstadual;
            Resultado.AceitaRemanejamento = existente.AceitaRemanejamento;
            Resultado.ObsRemanejamento    = existente.ObsRemanejamento;
            Resultado.Valor               = existente.Valor;
            Resultado.Pago                = existente.Pago;
        }

        Text            = Resultado.Id == 0 ? "Nova Inscrição" : "Editar Inscrição";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize    = new Size(490, 500);

        _atletas    = AtletasService.ListarComEntidade().ToList();
        _categorias = CategoriasService.Listar().ToList();

        MontarLayout();

        // Detecta tipo se editando
        if (Resultado.CategoriaId > 0)
        {
            var cat = _categorias.FirstOrDefault(c => c.Id == Resultado.CategoriaId);
            if (cat != null) _tipo = cat.Tipo;
        }

        // Configura bindings e preenche dados
        ConfigurarAtletaBindings();
        AplicarTipo(_tipo, resetAtleta2: false);
        PreencherExistente();
    }

    // ── Layout ───────────────────────────────────────────────────────────────

    private void MontarLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            Padding     = new Padding(16, 10, 16, 4),
            ColumnCount = 2,
            RowCount    = 12,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 118));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        int[] alturas = [58, 34, 20, 34, 20, 34, 34, 34, 34, 34, 34, 0];
        for (int i = 0; i < alturas.Length - 1; i++)
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, alturas[i]));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Linha 0: Modalidade
        var tipoLabel = new Label
        {
            Text      = "Modalidade:",
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleRight,
            ForeColor = Estilos.TextPrimary,
        };
        var tipoFlow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding       = new Padding(0, 11, 0, 0),
        };
        tipoFlow.Controls.AddRange(new Control[] { _btnSimples, _btnDupla, _btnMista });
        layout.Controls.Add(tipoLabel, 0, 0);
        layout.Controls.Add(tipoFlow,  1, 0);

        // Linha 1-2: Atleta 1
        _cmbAtleta1.Dock = DockStyle.Fill;
        _lblFaixa1.Dock  = DockStyle.Fill;
        layout.Controls.Add(RotuloLabel("Atleta 1:"), 0, 1);
        layout.Controls.Add(_cmbAtleta1, 1, 1);
        layout.Controls.Add(_lblFaixa1,  0, 2);
        layout.SetColumnSpan(_lblFaixa1, 2);

        // Linha 3-4: Atleta 2
        _cmbAtleta2.Dock    = DockStyle.Fill;
        _lblAtleta2.Dock    = DockStyle.Fill;
        _lblFaixa2.Dock     = DockStyle.Fill;
        layout.Controls.Add(_lblAtleta2, 0, 3);
        layout.Controls.Add(_cmbAtleta2, 1, 3);
        layout.Controls.Add(_lblFaixa2,  0, 4);
        layout.SetColumnSpan(_lblFaixa2, 2);

        // Linha 5: Categoria
        _cmbCategoria.Dock = DockStyle.Fill;
        layout.Controls.Add(RotuloLabel("Categoria:"), 0, 5);
        layout.Controls.Add(_cmbCategoria, 1, 5);

        // Linha 6-7: RK
        layout.Controls.Add(RotuloLabel("RK Interno:"),  0, 6); layout.Controls.Add(_nudRkInterno,  1, 6);
        layout.Controls.Add(RotuloLabel("RK Estadual:"), 0, 7); layout.Controls.Add(_nudRkEstadual, 1, 7);

        // Linha 8-9: Remanejamento
        layout.Controls.Add(RotuloLabel("Remanejamento:"), 0, 8); layout.Controls.Add(_chkRemanej,    1, 8);
        _txtObsRemanej.Dock = DockStyle.Fill;
        layout.Controls.Add(RotuloLabel("Obs. remanej.:"), 0, 9); layout.Controls.Add(_txtObsRemanej, 1, 9);

        // Linha 10: Valor + Pago
        var valorFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight };
        var lblPago   = new Label { Text = "    Pago:", AutoSize = false, Width = 60, Height = 28, TextAlign = ContentAlignment.MiddleLeft };
        valorFlow.Controls.AddRange(new Control[] { _nudValor, lblPago, _chkPago });
        layout.Controls.Add(RotuloLabel("Valor (R$):"), 0, 10);
        layout.Controls.Add(valorFlow, 1, 10);

        // Botões
        var btnSalvar   = Estilos.CriarBotao("Salvar",   "novo");
        var btnCancelar = Estilos.CriarBotao("Cancelar", "padrao");
        btnCancelar.DialogResult = DialogResult.Cancel;
        btnSalvar.Click += Salvar_Click;

        var barraBtns = Estilos.CriarBarraBotoes(btnSalvar, btnCancelar);

        Controls.Add(layout);
        Controls.Add(barraBtns);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        // Eventos
        _btnSimples.Click += (_, _) => AplicarTipo("SIMPLES");
        _btnDupla.Click   += (_, _) => AplicarTipo("DUPLA");
        _btnMista.Click   += (_, _) => AplicarTipo("MISTA");
        _cmbAtleta1.SelectedIndexChanged += (_, _) => { AtualizarFaixa1(); AtualizarCategoriaAuto(); };
        _cmbAtleta2.SelectedIndexChanged += (_, _) => AtualizarFaixa2();
    }

    // ── Bindings de atletas ──────────────────────────────────────────────────

    private void ConfigurarAtletaBindings()
    {
        _cmbAtleta1.DisplayMember = "NomeCompleto";
        _cmbAtleta1.ValueMember   = "Id";
        _cmbAtleta1.DataSource    = _atletas.ToList();

        var comNenhum = new List<Atleta> { new() { Id = 0, NomeCompleto = "(nenhum)" } };
        comNenhum.AddRange(_atletas);
        _cmbAtleta2.DisplayMember = "NomeCompleto";
        _cmbAtleta2.ValueMember   = "Id";
        _cmbAtleta2.DataSource    = comNenhum;
    }

    // ── Tipo de modalidade ───────────────────────────────────────────────────

    private void AplicarTipo(string tipo, bool resetAtleta2 = true)
    {
        _tipo = tipo;

        // Estilo dos botões de toggle
        foreach (var (btn, t) in new[] { (_btnSimples, "SIMPLES"), (_btnDupla, "DUPLA"), (_btnMista, "MISTA") })
        {
            btn.BackColor = t == tipo ? Estilos.AccentBlue : Color.FromArgb(220, 225, 232);
            btn.ForeColor = t == tipo ? Color.White : Estilos.TextPrimary;
        }

        bool temAtleta2 = tipo != "SIMPLES";
        _cmbAtleta2.Enabled       = temAtleta2;
        _lblAtleta2.ForeColor     = temAtleta2 ? Estilos.TextPrimary : Color.Silver;
        _lblFaixa2.Text           = temAtleta2 ? _lblFaixa2.Text : "";

        if (!temAtleta2 && resetAtleta2)
            _cmbAtleta2.SelectedIndex = 0;  // "(nenhum)"

        // Recarrega categorias filtradas por tipo
        _categoriasDoTipo = _categorias.Where(c => c.Tipo == tipo).ToList();
        _cmbCategoria.DisplayMember = "Codigo";
        _cmbCategoria.ValueMember   = "Id";
        _cmbCategoria.DataSource    = _categoriasDoTipo.ToList();

        AtualizarCategoriaAuto();
    }

    // ── Faixas etárias ───────────────────────────────────────────────────────

    private void AtualizarFaixa1()
    {
        var a = _cmbAtleta1.SelectedItem as Atleta;
        if (a == null || a.Id <= 0) { _lblFaixa1.Text = ""; return; }
        _lblFaixa1.Text = $"  → {FaixaLabel(a.AnoNascimento)}  ·  nasc. {a.AnoNascimento}  ·  {a.Sexo}  ·  {a.Entidade?.Sigla ?? ""}";
    }

    private void AtualizarFaixa2()
    {
        var a = _cmbAtleta2.SelectedItem as Atleta;
        if (a == null || a.Id <= 0) { _lblFaixa2.Text = ""; return; }
        _lblFaixa2.Text = $"  → {FaixaLabel(a.AnoNascimento)}  ·  nasc. {a.AnoNascimento}  ·  {a.Sexo}  ·  {a.Entidade?.Sigla ?? ""}";
    }

    // ── Auto-detecção de categoria ───────────────────────────────────────────

    private void AtualizarCategoriaAuto()
    {
        var a1 = _cmbAtleta1.SelectedItem as Atleta;
        if (a1 == null || a1.Id <= 0) return;

        string codigo = CodigoCategoria(a1);
        var cat = _categoriasDoTipo.FirstOrDefault(c => c.Codigo == codigo);
        if (cat != null) _cmbCategoria.SelectedValue = cat.Id;
    }

    private string CodigoCategoria(Atleta a1)
    {
        string faixa = a1.AnoNascimento switch
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
            "SIMPLES" => a1.Sexo == "M" ? "SM" : "SF",
            "DUPLA"   => a1.Sexo == "M" ? "DM" : "DF",
            _         => "DX",  // MISTA
        };

        return prefixo + faixa;
    }

    private string FaixaLabel(int anoNasc) => anoNasc switch
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

    // ── Preencher ao editar ──────────────────────────────────────────────────

    private void PreencherExistente()
    {
        if (Resultado.Atleta1Id > 0)           _cmbAtleta1.SelectedValue   = Resultado.Atleta1Id;
        if (Resultado.Atleta2Id is int a2Id)   _cmbAtleta2.SelectedValue   = a2Id;
        if (Resultado.CategoriaId > 0)         _cmbCategoria.SelectedValue = Resultado.CategoriaId;

        _nudRkInterno.Value  = Resultado.RkInterno  ?? 0;
        _nudRkEstadual.Value = Resultado.RkEstadual ?? 0;
        _chkRemanej.Checked  = Resultado.AceitaRemanejamento;
        _txtObsRemanej.Text  = Resultado.ObsRemanejamento ?? string.Empty;
        _nudValor.Value      = (decimal)Resultado.Valor;
        _chkPago.Checked     = Resultado.Pago;

        AtualizarFaixa1();
        AtualizarFaixa2();
    }

    // ── Salvar ───────────────────────────────────────────────────────────────

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.CategoriaId         = _cmbCategoria.SelectedValue is int cid ? cid : 0;
        Resultado.Atleta1Id           = _cmbAtleta1.SelectedValue  is int a1  ? a1  : 0;
        int a2                        = _cmbAtleta2.SelectedValue  is int v2  ? v2  : 0;
        Resultado.Atleta2Id           = a2 > 0 ? a2 : null;
        Resultado.RkInterno           = (int)_nudRkInterno.Value  > 0 ? (int)_nudRkInterno.Value  : null;
        Resultado.RkEstadual          = (int)_nudRkEstadual.Value > 0 ? (int)_nudRkEstadual.Value : null;
        Resultado.AceitaRemanejamento = _chkRemanej.Checked;
        Resultado.ObsRemanejamento    = string.IsNullOrWhiteSpace(_txtObsRemanej.Text) ? null : _txtObsRemanej.Text.Trim();
        Resultado.Valor               = (double)_nudValor.Value;
        Resultado.Pago                = _chkPago.Checked;

        if (Resultado.CategoriaId <= 0) { Aviso("Selecione uma categoria."); return; }
        if (Resultado.Atleta1Id   <= 0) { Aviso("Selecione o Atleta 1.");    return; }
        if (_tipo != "SIMPLES" && Resultado.Atleta2Id == null)
            { Aviso($"Para {_tipo}, selecione o Atleta 2."); return; }
        if (Resultado.Atleta2Id.HasValue && Resultado.Atleta2Id == Resultado.Atleta1Id)
            { Aviso("Atleta 1 e Atleta 2 não podem ser o mesmo."); return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    // ── Helpers de layout ────────────────────────────────────────────────────

    private static Button TipoBotao(string texto)
    {
        var btn = new Button
        {
            Text      = texto,
            Width     = 104,
            Height    = 34,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(220, 225, 232),
            ForeColor = Estilos.TextPrimary,
            Cursor    = Cursors.Hand,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f),
            Margin    = new Padding(0, 0, 6, 0),
        };
        btn.FlatAppearance.BorderSize = 0;
        return btn;
    }

    private static Label InfoLabel() => new()
    {
        Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 8.5f),
        ForeColor = Color.FromArgb(70, 120, 160),
        TextAlign = ContentAlignment.MiddleLeft,
    };

    private static Label RotuloLabel(string texto) =>
        new() { Text = texto, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
