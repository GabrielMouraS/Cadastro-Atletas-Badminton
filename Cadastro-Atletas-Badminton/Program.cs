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
        Application.Run(new MainForm());
    }
}