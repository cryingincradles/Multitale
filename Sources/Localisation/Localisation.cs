#pragma warning disable

using Newtonsoft.Json;

namespace Multitale.Sources.Localisation;

public class Localisation
{
    public static Base DefaultLocale = new English();
    public static string DefaultLocaleName = DefaultLocale.GetType().Name;
    private static readonly Dictionary<string, Base> LocalesCache = new();
    
    public static void Build()
    {
        var manager = new Manager();
        manager.Load();
    }
    
    public class HomeMenu
    {
        public string Tint { get; set; }
        public string Home { get; set; }
        public string Launcher { get; set; }
        public string Settings { get; set; }
        public string About { get; set; }
        public string Donate { get; set; }
    }

    public class AboutMenu
    {
        public string Tint { get; set; }
        public string About { get; set; }
        public string AboutText { get; set; }
        public string Links { get; set; }
        public string LinksText { get; set; }
        public string Developers { get; set; }
        public string DevelopersText { get; set; }
        public string GoBack { get; set; }
    }

    public class DonateMenu
    {
        public string Tint { get; set; }
        public string Donate { get; set; }
        public string DonateText { get; set; }
        public string Requisites { get; set; }
        public string RequisitesText { get; set; }
    }

    public class SettingsMenu
    {
        public string Tint { get; set; }
        public string Settings { get; set; }
        public string Language { get; set; }
        public string Theme { get; set; }
        public string ViewMode { get; set; }
        public string SaveDetails { get; set; }
        public string ProxyPath { get; set; }
        public string ProxyTimeout { get; set; }
        public string GoBack { get; set; }
        public string Change { get; set; }
        public string CurrentValue { get; set; }
    }

    public class LauncherMenu
    {
        public string Tint { get; set; }
        public string Launcher { get; set; }
        public string Decoder { get; set; }
        public string Fetcher { get; set; }
        public string ProxyScrapper { get; set; }
        public string ProxyChecker { get; set; }
    }

    public class Base
    {
        public string Alias { get; set; }
        public HomeMenu HomeMenu { get; set; }
        public LauncherMenu LauncherMenu { get; set; }
        public SettingsMenu SettingsMenu { get; set; }
        public AboutMenu AboutMenu { get; set; }
        public DonateMenu DonateMenu { get; set; }
    }
    
    public class Manager
    {
        public static readonly string LocalePath = "./Localisation";
        private readonly object _fileLocker = new();

        public Base GetLocale(string localeName)
        {
            localeName += ".json";
            string? localeString;
            
            if (LocalesCache.TryGetValue(localeName, out var cachedLocale))
                return cachedLocale;
            
            try
            {
                localeString = File.ReadAllText(Path.Combine(LocalePath, localeName));
            }
            
            catch (Exception ex)
            {
                if (localeName != DefaultLocaleName)
                {
                    Program.Log.Error($"Error occured while trying to read {localeName} file\n{ex}");
                    Program.Log.Warning("Default localisation will be used");
                }
                
                if (!LocalesCache.ContainsKey(DefaultLocaleName))
                    CreateDefaultLocale();
                return LocalesCache[DefaultLocaleName];
            }

            var locale = JsonConvert.DeserializeObject<Base>(localeString);
            if (locale is null)
                Program.Log.Warning($"Loaded localisation file ({localeName}) structure is different from Base structure. Default \"{DefaultLocaleName}\" localisation will be used");

            return locale ?? LocalesCache[DefaultLocaleName];
        }

        public void Load()
        {
            Program.Log.Information("Checking default localisation(-s)");
            CreateDefaultLocale();
            CreateDefaultLocale(new Russian());
        }

        private void CreateDefaultLocale(Base? additionalLocale = null)
        {
            lock (_fileLocker)
            {
                var locale = DefaultLocale;
                if (additionalLocale is not null)
                    locale = additionalLocale;
                var localeName = locale.GetType().Name;
                
                if (!Directory.Exists(LocalePath))
                    Directory.CreateDirectory(LocalePath);

                var localeFilePath = Path.Combine(LocalePath, $"{localeName}.json");
                
                if (!LocalesCache.ContainsKey(localeName))
                {
                    Program.Log.Information($"Saving {localeName} localisation to program cache...");
                    
                    LocalesCache.Add(localeName, locale);
                }
                
                if (File.Exists(localeFilePath)) return;

                Program.Log.Information($"Creating {localeName} localisation file...");

                var defaultLocale = LocalesCache[localeName];
                var defaultLocaleString = JsonConvert.SerializeObject(defaultLocale, Formatting.Indented);

                File.WriteAllText(localeFilePath, defaultLocaleString);
            }
        }
    }
}