namespace BadmintonCadastro.UI.Components;

internal static class Estilos
{
    // ── Paleta ───────────────────────────────────────────────────────────────
    public static readonly Color SidebarBg      = Color.FromArgb(28, 42, 58);
    public static readonly Color SidebarText    = Color.FromArgb(160, 174, 192);
    public static readonly Color SidebarActiveBg = Color.FromArgb(52, 152, 219);
    public static readonly Color ContentBg      = Color.FromArgb(245, 247, 250);
    public static readonly Color AccentGreen    = Color.FromArgb(39, 174, 96);
    public static readonly Color AccentBlue     = Color.FromArgb(52, 152, 219);
    public static readonly Color AccentRed      = Color.FromArgb(192, 57, 43);
    public static readonly Color AccentOrange   = Color.FromArgb(211, 84, 0);
    public static readonly Color AccentPurple   = Color.FromArgb(142, 68, 173);
    public static readonly Color TextPrimary    = Color.FromArgb(44, 62, 80);
    public static readonly Color BorderColor    = Color.FromArgb(220, 225, 232);

    // ── Grid ─────────────────────────────────────────────────────────────────
    public static void EstilizarGrid(DataGridView grid)
    {
        grid.BorderStyle                = BorderStyle.None;
        grid.CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor                  = BorderColor;
        grid.BackgroundColor           = Color.White;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersHeight       = 38;
        grid.ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.None;
        grid.RowTemplate.Height        = 30;

        grid.ColumnHeadersDefaultCellStyle.BackColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9.5f, FontStyle.Bold);
        grid.ColumnHeadersDefaultCellStyle.Padding   = new Padding(8, 0, 8, 0);

        grid.DefaultCellStyle.BackColor          = Color.White;
        grid.DefaultCellStyle.ForeColor          = TextPrimary;
        grid.DefaultCellStyle.Font               = new Font(SystemFonts.DefaultFont!.FontFamily, 9.5f);
        grid.DefaultCellStyle.SelectionBackColor = AccentBlue;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.DefaultCellStyle.Padding            = new Padding(8, 0, 8, 0);

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 246);
    }

    // ── Botões de ação ───────────────────────────────────────────────────────
    public static Button CriarBotao(string texto, string tipo = "padrao")
    {
        Color cor = tipo switch
        {
            "novo"       => AccentGreen,
            "editar"     => AccentBlue,
            "excluir"    => AccentRed,
            "exportar"   => AccentPurple,
            "inscricoes" => AccentOrange,
            _            => Color.FromArgb(149, 165, 166),
        };

        var btn = new Button
        {
            Text      = texto,
            Height    = 34,
            Width     = texto.Length > 9 ? 124 : 100,
            FlatStyle = FlatStyle.Flat,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9f),
            BackColor = cor,
            ForeColor = Color.White,
            Cursor    = Cursors.Hand,
        };
        btn.FlatAppearance.BorderSize         = 0;
        btn.FlatAppearance.MouseOverBackColor = Escurecer(cor, 18);
        btn.FlatAppearance.MouseDownBackColor = Escurecer(cor, 35);
        return btn;
    }

    // ── Cabeçalho de seção ───────────────────────────────────────────────────
    public static Panel CriarHeader(string titulo, string? detalhe = null)
    {
        var panel = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 58,
            BackColor = Color.White,
            Padding   = new Padding(20, 0, 20, 0),
        };
        panel.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(BorderColor), 0, panel.Height - 1, panel.Width, panel.Height - 1);

        var lbl = new Label
        {
            Text      = detalhe != null ? $"{titulo}   —   {detalhe}" : titulo,
            Dock      = DockStyle.Fill,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 13f, FontStyle.Bold),
            ForeColor = TextPrimary,
            TextAlign = ContentAlignment.MiddleLeft,
        };
        panel.Controls.Add(lbl);
        return panel;
    }

    // ── Barra de botões de ação ───────────────────────────────────────────────
    public static Panel CriarBarraBotoes(params Button[] botoes)
    {
        var panel = new Panel
        {
            Dock      = DockStyle.Bottom,
            Height    = 54,
            BackColor = Color.White,
        };
        panel.Paint += (_, e) =>
            e.Graphics.DrawLine(new Pen(BorderColor), 0, 0, panel.Width, 0);

        var flow = new FlowLayoutPanel
        {
            Dock          = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding       = new Padding(12, 10, 12, 0),
        };
        foreach (var btn in botoes) flow.Controls.Add(btn);
        panel.Controls.Add(flow);
        return panel;
    }

    private static Color Escurecer(Color c, int d) =>
        Color.FromArgb(Math.Max(0, c.R - d), Math.Max(0, c.G - d), Math.Max(0, c.B - d));
}
