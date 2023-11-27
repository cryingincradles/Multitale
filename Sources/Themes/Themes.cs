#pragma warning disable

using Multitale.Sources.Helpers;

namespace Multitale.Sources.Themes;

public class Themes
{
    public static Base DefaultTheme = new Purple();
    public static string DefaultThemeName = DefaultTheme.GetType().Name;
    
    public static void Build()
    {
        var manager = new Manager();
        manager.Load();
    }
    
    public class Base
    {
        public string Alias { get; set; }
        public string DefaultColor { get; set; }
        public string TintColor { get; set; }
        public string AccentColor { get; set; }
        public string BaseColor { get; set; }
        public string ErrorColor { get; set; }
        public string WarningColor { get; set; }
    }

    public class Manager : JsonManager<Base>
    {
        public const string ThemesPath = "./Themes";
        
        public Manager() : base(ThemesPath, new Purple(), "theme") { }

        public static Base? GetThemeFromFile(string themeName) => GetFromFile(themeName);

        public Base GetTheme(string themeName) => Get(themeName);

        public void Load() => base.Load();
    }
}