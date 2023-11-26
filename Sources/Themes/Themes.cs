#pragma warning disable

using Newtonsoft.Json;

namespace Multitale.Sources.Themes;

public class Themes
{
    public static Base DefaultTheme = new Purple();
    private static readonly Dictionary<string, Base> ThemesCache = new();
    
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

    public class Manager
    {
        public static readonly string ThemesPath = "./Themes";
        private readonly object _fileLocker = new();

        public Base GetTheme(string themeName)
        {
            themeName += ".json";
            string? themeString;
            
            if (ThemesCache.TryGetValue(themeName, out var cachedTheme))
                return cachedTheme;

            try
            {
                themeString = File.ReadAllText(Path.Combine(ThemesPath, themeName));
            }

            catch (Exception ex)
            {
                if (themeName != DefaultTheme.GetType().Name)
                {
                    Program.Log.Error($"Error occured while trying to read {themeName} file\n{ex}");
                    Program.Log.Warning("Default theme will be used");
                }
                
                if (!ThemesCache.ContainsKey(DefaultTheme.GetType().Name))
                    CreateDefaultTheme();
                return ThemesCache[DefaultTheme.GetType().Name];
            }
            
            var theme = JsonConvert.DeserializeObject<Base>(themeString);
            if (theme is null)
                Program.Log.Warning($"Loaded localisation file ({themeName}) structure is different from Base structure. Default \"{DefaultTheme.GetType().Name}\" localisation will be used");

            return theme ?? ThemesCache[DefaultTheme.GetType().Name];
        }

        public void Load()
        {
            Program.Log.Information("Checking default theme");
            CreateDefaultTheme();
        }
        
        private void CreateDefaultTheme(Base? additionalTheme = null)
        {
            lock (_fileLocker)
            {
                var theme = DefaultTheme;
                if (additionalTheme is not null)
                    theme = additionalTheme;
                var themeName = theme.GetType().Name;
                
                if (!Directory.Exists(ThemesPath))
                    Directory.CreateDirectory(ThemesPath);

                var themeFilePath = Path.Combine(ThemesPath, $"{themeName}.json");
                
                if (!ThemesCache.ContainsKey(themeName))
                {
                    Program.Log.Information($"Saving {themeName} theme to program cache");
                    ThemesCache.Add(themeName, theme);
                }
                
                if (File.Exists(themeFilePath)) return;
                
                ThemesCache.Add(themeName, theme);

                Program.Log.Information($"Creating {themeName} theme file");
                
                var defaultTheme = ThemesCache[themeName];
                var defaultThemeString = JsonConvert.SerializeObject(defaultTheme, Formatting.Indented);
                
                File.WriteAllText(themeFilePath, defaultThemeString);
            }
        }
    }
}