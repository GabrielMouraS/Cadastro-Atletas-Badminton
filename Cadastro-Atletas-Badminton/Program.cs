using BadmintonCadastro.Services;
using BadmintonCadastro.UI.Forms;

namespace BadmintonCadastro;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseService.Initialize();

        // Splash: abre, aguarda fechar, então abre o app principal
        using var splash = new SplashForm();
        splash.Show();
        // Processa eventos até o splash fechar sozinho
        while (splash.Visible)
            Application.DoEvents();

        Application.Run(new MainForm());
    }
}