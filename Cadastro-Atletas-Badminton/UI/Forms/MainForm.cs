using BadmintonCadastro.UI.Components;

namespace BadmintonCadastro.UI.Forms;

internal class MainForm : Form
{
    private readonly Panel  _sidebar      = new() { Width = 210 };
    private readonly Panel  _contentPanel = new();
    private          Button? _navAtivo;

    public MainForm()
    {
        Text          = "Badminton — Cadastro de Atletas";
        ClientSize    = new System.Drawing.Size(1100, 660);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize   = new System.Drawing.Size(820, 520);
        BackColor     = Estilos.ContentBg;

        ConfigurarSidebar();

        _contentPanel.Dock      = DockStyle.Fill;
        _contentPanel.BackColor = Estilos.ContentBg;
        _contentPanel.Padding   = new Padding(16);

        // contentPanel (Fill) added first, sidebar (Left) added second → sidebar docks first
        Controls.Add(_contentPanel);
        Controls.Add(_sidebar);

        NavegarPara(new TorneiosForm());
    }

    // ── Sidebar ──────────────────────────────────────────────────────────────

    private void ConfigurarSidebar()
    {
        _sidebar.Dock      = DockStyle.Left;
        _sidebar.BackColor = Estilos.SidebarBg;

        var header = new Panel { Height = 68, BackColor = Color.FromArgb(18, 28, 40) };
        var lblApp = new Label
        {
            Text      = "Badminton",
            Dock      = DockStyle.Fill,
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 12f, FontStyle.Bold),
        };
        header.Controls.Add(lblApp);

        var lblCad        = SecaoLabel("CADASTROS");
        var btnEntidades  = NavBotao("Entidades");
        var btnAtletas    = NavBotao("Atletas");
        var btnCategorias = NavBotao("Categorias");

        var lblComp    = SecaoLabel("COMPETIÇÕES");
        var btnTorneios = NavBotao("Torneios");

        btnEntidades.Click  += (_, _) => { Ativar(btnEntidades);  NavegarPara(new EntidadesForm()); };
        btnAtletas.Click    += (_, _) => { Ativar(btnAtletas);    NavegarPara(new AtletasForm()); };
        btnCategorias.Click += (_, _) => { Ativar(btnCategorias); NavegarPara(new CategoriasForm()); };
        btnTorneios.Click   += (_, _) => { Ativar(btnTorneios);   NavegarPara(new TorneiosForm()); };

        // Adicionados em ordem INVERSA à visual: último adicionado (z=0) é processado primeiro
        // pelo dock layout e fica no topo. Portanto: header (topo) deve ser adicionado por último.
        _sidebar.Controls.Add(btnTorneios);
        _sidebar.Controls.Add(lblComp);
        _sidebar.Controls.Add(btnCategorias);
        _sidebar.Controls.Add(btnAtletas);
        _sidebar.Controls.Add(btnEntidades);
        _sidebar.Controls.Add(lblCad);
        _sidebar.Controls.Add(header);
    }

    private void NavegarPara(Form form)
    {
        foreach (Control c in _contentPanel.Controls)
            if (c is Form f) f.Hide();
        _contentPanel.Controls.Clear();

        form.TopLevel        = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock            = DockStyle.Fill;
        _contentPanel.Controls.Add(form);
        form.Show();
    }

    private void Ativar(Button btn)
    {
        if (_navAtivo != null)
        {
            _navAtivo.BackColor = Estilos.SidebarBg;
            _navAtivo.ForeColor = Estilos.SidebarText;
        }
        btn.BackColor = Estilos.SidebarActiveBg;
        btn.ForeColor = Color.White;
        _navAtivo = btn;
    }

    // ── Helpers visuais da sidebar ───────────────────────────────────────────

    private static Button NavBotao(string texto)
    {
        var btn = new Button
        {
            Text      = texto,
            Dock      = DockStyle.Top,
            Height    = 44,
            FlatStyle = FlatStyle.Flat,
            BackColor = Estilos.SidebarBg,
            ForeColor = Estilos.SidebarText,
            TextAlign = ContentAlignment.MiddleLeft,
            Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 9.5f),
            Cursor    = Cursors.Hand,
            Padding   = new Padding(16, 0, 0, 0),
        };
        btn.FlatAppearance.BorderSize         = 0;
        btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 58, 78);
        btn.FlatAppearance.MouseDownBackColor = Estilos.SidebarActiveBg;
        return btn;
    }

    private static Label SecaoLabel(string texto) => new()
    {
        Text      = texto,
        Dock      = DockStyle.Top,
        Height    = 38,
        ForeColor = Color.FromArgb(95, 116, 138),
        BackColor = Estilos.SidebarBg,
        TextAlign = ContentAlignment.BottomLeft,
        Font      = new Font(SystemFonts.DefaultFont!.FontFamily, 7.5f, FontStyle.Bold),
        Padding   = new Padding(16, 0, 0, 6),
    };
}
