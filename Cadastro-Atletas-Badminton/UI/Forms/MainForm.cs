namespace BadmintonCadastro.UI.Forms;

internal class MainForm : Form
{
    public MainForm()
    {
        Text = "Cadastro de Atletas — Badminton";
        ClientSize = new System.Drawing.Size(960, 620);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new System.Drawing.Size(700, 480);

        var menu = new MenuStrip();

        var mCadastros = new ToolStripMenuItem("&Cadastros");
        mCadastros.DropDownItems.Add("&Entidades",  null, (_, _) => new EntidadesForm().ShowDialog(this));
        mCadastros.DropDownItems.Add("&Atletas",    null, (_, _) => new AtletasForm().ShowDialog(this));
        mCadastros.DropDownItems.Add("&Categorias", null, (_, _) => new CategoriasForm().ShowDialog(this));

        var mTorneios = new ToolStripMenuItem("&Torneios");
        mTorneios.DropDownItems.Add("&Gerenciar Torneios", null, (_, _) => new TorneiosForm().ShowDialog(this));

        menu.Items.Add(mCadastros);
        menu.Items.Add(mTorneios);
        Controls.Add(menu);
        MainMenuStrip = menu;

        var status = new StatusStrip();
        status.Items.Add(new ToolStripStatusLabel("Sistema de Cadastro de Atletas — Badminton"));
        Controls.Add(status);

        var lblBem = new Label
        {
            Text = "Bem-vindo ao Sistema de Cadastro de Atletas\n\nUtilize o menu para navegar.",
            Dock = DockStyle.Fill,
            TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
            Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont!.FontFamily, 12f)
        };
        Controls.Add(lblBem);
    }
}
