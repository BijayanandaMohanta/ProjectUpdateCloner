namespace ProjectUpdateCloner;

public partial class App : System.Windows.Application
{
    private static readonly string LogPath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ProjectUpdateCloner",
        "startup.log");

    public App()
    {
        Log("App constructed.");
        Startup += (_, _) =>
        {
            Helpers.SystemThemeManager.Start(this);
            Log("Application startup event.");
        };
        DispatcherUnhandledException += (_, args) =>
        {
            Log("Dispatcher exception: " + args.Exception);
        };
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            Log("AppDomain exception: " + args.ExceptionObject);
        };
    }

    public static void Log(string message)
    {
        try
        {
            var directory = System.IO.Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            System.IO.File.AppendAllText(LogPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
        catch
        {
            // Startup logging must never prevent the GUI from opening.
        }
    }
}
