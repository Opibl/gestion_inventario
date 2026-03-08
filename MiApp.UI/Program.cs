using DotNetEnv;
using MiApp.UI;

namespace MiApp.UI;

static class Program
{
    [STAThread]
    static void Main()
    {
        // 🔹 Cargar variables .env
        Env.Load();

        // 🔹 Manejo global de errores UI
        Application.ThreadException += (sender, args) =>
        {
            LogError(args.Exception);
            MessageBox.Show(
                "Ocurrió un error inesperado.\nRevise errors.log",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        };

        // 🔹 Manejo errores tareas async
        AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                LogError(ex);
            }
        };

        ApplicationConfiguration.Initialize();

        while (true)
        {
            try
            {
                using var login = new LoginForm();

                // Si cancela login → cerrar
                if (login.ShowDialog() != DialogResult.OK)
                    break;

                // Ejecutar sistema
                Application.Run(new Form1(login.AuthenticatedUser!));
            }
            catch (Exception ex)
            {
                LogError(ex);

                MessageBox.Show(
                    "Error crítico en la aplicación.\nRevise errors.log",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }

    private static void LogError(Exception ex)
    {
        try
        {
            var log = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {ex}\n\n";
            File.AppendAllText("errors.log", log);
        }
        catch
        {
            // evitar crash si falla el log
        }
    }
}