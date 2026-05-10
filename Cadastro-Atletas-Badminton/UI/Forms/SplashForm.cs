namespace BadmintonCadastro.UI.Forms;

internal class SplashForm : Form
{
    private readonly System.Windows.Forms.Timer _timer  = new() { Interval = 16 }; // ~60 fps
    private readonly Panel  _barra  = new();
    private readonly Label  _lblApp = new();
    private readonly Label  _lblSub = new();

    private double _opacidade   = 0;
    private int    _barraPx     = 0;
    private int    _fase        = 0;   // 0=fade-in  1=espera  2=fade-out
    private int    _espera      = 0;

    private const int BarraLarg   = 480;
    private const int FadeInStep  = 12;   // ticks para fade-in
    private const int EsperaMax   = 60;   // ticks de espera
    private const int FadeOutStep = 10;   // ticks para fade-out

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition   = FormStartPosition.CenterScreen;
        ClientSize      = new Size(520, 280);
        BackColor       = Color.FromArgb(22, 33, 47);
        Opacity         = 0;
        TopMost         = true;
        ShowInTaskbar   = false;

        // Título principal
        _lblApp.Text      = "BADMINTON";
        _lblApp.Font      = new Font("Segoe UI", 36f, FontStyle.Bold);
        _lblApp.ForeColor = Color.White;
        _lblApp.AutoSize  = true;
        _lblApp.Location  = new Point(0, 90);

        // Subtítulo — posicionado dinamicamente no Load para não sobrepor
        _lblSub.Text      = "Cadastro de Atletas";
        _lblSub.Font      = new Font("Segoe UI", 13f, FontStyle.Regular);
        _lblSub.ForeColor = Color.FromArgb(100, 160, 220);
        _lblSub.AutoSize  = true;
        _lblSub.Location  = new Point(0, 160);

        // Linha de progresso (começa com 0px de largura)
        _barra.BackColor = Color.FromArgb(52, 152, 219);
        _barra.Height    = 3;
        _barra.Width     = 0;
        _barra.Top       = ClientSize.Height - 3;
        _barra.Left      = 0;

        // Linha fundo da barra (trilha cinza)
        var trilha = new Panel
        {
            BackColor = Color.FromArgb(40, 60, 80),
            Height    = 3,
            Width     = ClientSize.Width,
            Top       = ClientSize.Height - 3,
            Left      = 0,
        };

        Controls.AddRange(new Control[] { trilha, _barra, _lblApp, _lblSub });

        Load += (_, _) =>
        {
            // Força cálculo de tamanho antes de centralizar
            _lblApp.PerformLayout();
            _lblSub.PerformLayout();

            _lblApp.Left = (ClientSize.Width - _lblApp.Width) / 2;

            // Subtítulo sempre 10px abaixo do bottom do título
            _lblSub.Top  = _lblApp.Bottom + 10;
            _lblSub.Left = (ClientSize.Width - _lblSub.Width) / 2;

            _timer.Tick += Tick;
            _timer.Start();
        };
    }

    private void Tick(object? sender, EventArgs e)
    {
        switch (_fase)
        {
            case 0: // Fade-in + barra crescendo
                _opacidade = Math.Min(1.0, _opacidade + 1.0 / FadeInStep);
                _barraPx   = (int)(_opacidade * BarraLarg);
                Opacity    = _opacidade;
                _barra.Width = _barraPx;
                if (_opacidade >= 1.0) _fase = 1;
                break;

            case 1: // Barra continua crescendo até o fim
                _barraPx = Math.Min(ClientSize.Width, _barraPx + ClientSize.Width / EsperaMax);
                _barra.Width = _barraPx;
                _espera++;
                if (_espera >= EsperaMax) _fase = 2;
                break;

            case 2: // Fade-out
                _opacidade = Math.Max(0.0, _opacidade - 1.0 / FadeOutStep);
                Opacity    = _opacidade;
                if (_opacidade <= 0)
                {
                    _timer.Stop();
                    Close();
                }
                break;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _timer.Dispose();
        base.Dispose(disposing);
    }
}
