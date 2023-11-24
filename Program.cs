#pragma warning disable CS8618

using Multitale.Sources;
using Serilog;
using Spectre.Console;

namespace Multitale;

class Program
{
    public static ILogger Log { get; private set; }
    public static Settings Settings { get; set; }
    
    public const string Logo = @" |\/|     | _|_ o _|_  _. |  _ " + "\n" + 
                               @" |  | |_| |  |_ |  |_ (_| | (/_";
    
    private static void BuildConsole()
    {
        AnsiConsole.Clear();
        AnsiConsole.Cursor.Hide();
        Console.CursorVisible = false;
        AnsiConsole.MarkupLine($"\n[mediumpurple]{Logo}[/]");
    }
    
    private static void BuildLogger()
    {
        Log = new LoggerConfiguration()
            .WriteTo.File($"Logs/Log [session {DateTime.Now:yyyy.MM.dd HH-mm-ss}].txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] => {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    private static void BuildLocalisation() => Localisation.Build();

    private static void BuildSettings() => Settings = new Settings();

    public static Localisation.Base Locale
    {
        get
        {
            var localeManager = new Localisation.Manager();
            return Settings.Main.Language is null ? 
                Localisation.English : 
                localeManager.GetLocale(Settings.Main.Language);
        }
    }
    
    public static void Main()
    {
        BuildLogger();
        BuildSettings();
        BuildLocalisation();
        BuildConsole();
        
        Log.Information($"Multitale loaded!");
        
        Sources.Menus.Main.Show();
        Console.ReadKey(false);
    }
}