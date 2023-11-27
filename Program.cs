#pragma warning disable CS8618

using Multitale.Sources;
using Multitale.Sources.Localisation;
using Multitale.Sources.Themes;
using Serilog;
using Spectre.Console;

namespace Multitale;

class Program
{
    public static ILogger Log { get; private set; }
    public static Settings Settings { get; set; }
    
    public const string Logo = @" |\/|     | _|_ o _|_  _. |  _ " + "\n" + 
                               @" |  | |_| |  |_ |  |_ (_| | (/_";
    
    private static void BuildConsole(bool rebuild = false)
    {
        if (rebuild)
        {
            AnsiConsole.Write("\x1b[1;1H\x1b[0J");
            Log.Information("Updating console");
        }

        else
        {
            Log.Information("Setting up console");
            AnsiConsole.Clear();
            AnsiConsole.Cursor.Hide();

            Console.Title = "Multitale";
            Console.CursorVisible = false;
        }
        
        AnsiConsole.MarkupLine($"\n[{Theme.BaseColor}]{Logo}[/]");
    }
    
    private static void BuildLogger()
    {
        Log = new LoggerConfiguration()
            .WriteTo.File($"Logs/Log_{DateTime.Now:yyyy.MM.dd_HH-mm-ss}.txt",
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss:ms} [{Level:u3}] => {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
        Log.Information("Logger loaded");
    }

    private static void BuildLocalisation() => Localisation.Build();

    private static void BuildThemes() => Themes.Build();
    
    private static void BuildSettings() => Settings = new Settings();

    public static Localisation.Base Locale
    {
        get
        {
            var localeManager = new Localisation.Manager();
            if (Settings.Main.Language is null)
                return localeManager.GetLocale(Localisation.DefaultLocaleName);
            
            var language = localeManager.GetLocale(Settings.Main.Language);
            return language;
        }
    }
    
    public static Themes.Base Theme
    {
        get
        {
            var themeManager = new Themes.Manager();
            if (Settings.Main.Theme is null)
                return themeManager.GetTheme(Themes.DefaultTheme.GetType().Name);
            
            var language = themeManager.GetTheme(Settings.Main.Theme);
            return language;
        }
    }

    public static void RebuildConsole() => BuildConsole(true);
    
    public static void Main()
    {
        BuildLogger();
        BuildSettings();
        BuildLocalisation();
        BuildThemes();
        BuildConsole();
        
        Log.Information("Multitale loaded!");

        Sources.Menus.Home.Show();
        Console.ReadKey(false);
    }
}