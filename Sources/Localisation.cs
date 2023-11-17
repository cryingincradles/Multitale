#pragma warning disable

using Newtonsoft.Json;

namespace Multitale.Sources;

public class Localisation
{
    public static void Build()
    {
        var manager = new Manager();
        manager.Load();
    }
    
    public class MainMenu
    {
        public string Tint { get; set; }
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
    }

    public class SettingsMenu
    {
        public string Tint { get; set; }
        public string Language { get; set; }
        public string ViewMode { get; set; }
        public string SaveDetails { get; set; }
        public string ProxyPath { get; set; }
    }

    public class LauncherMenu
    {
        public string Tint { get; set; }
        public string Decoder { get; set; }
        public string Fetcher { get; set; }
        public string ProxyScrapper { get; set; }
        public string ProxyChecker { get; set; }
    }

    public class Base
    {
        public MainMenu MainMenu { get; set; }
        public LauncherMenu LauncherMenu { get; set; }
        public SettingsMenu SettingsMenu { get; set; }
        public AboutMenu AboutMenu { get; set; }
        public DonateMenu DonateMenu { get; set; }
    }

    public static Base English = new()
    {
        MainMenu = new()
        {
            Tint = "Use arrows to select and enter or space to confirm",
            About = "About",
            Launcher = "Launcher",
            Settings = "Settings",
            Donate = "Donate"
        },
        AboutMenu = new()
        {
            Tint = "Use Escape or Backspace to go back",
            About = "About",
            Developers = "Developers",
            AboutText = "I decided to make a project that could interest both me and people who know what it is for." +
                        "I wanted to try to make something that could be redesigned for myself and supplemented by any developer who would like to try himself in this field.",
            DevelopersText = "cradles saykowa",
            Links = "Links",
            LinksText = "Github: https://github.com/cryingincradles/Multitale Telegram: https://t.me/multitale Lolzteam: https://zelenka.guru/threads/5167800",
            GoBack = "Go back"
        },
        DonateMenu = new()
        {
            Tint = "Use Escape or Backspace to go back",
            Donate = "Donate",
            DonateText = "Multitale is my biggest development practice, to which I devote a huge amount of my time." +
                         "So, if you want the project to continue to be supported, updated, and supplemented, you can leave your donation using the available methods below"
        },
        LauncherMenu = new()
        {
            Tint = "Use arrows to select (or Escape/Backspace to go back)",
            Decoder = "Decoder",
            Fetcher = "Fetcher",
            ProxyScrapper = "Proxy Scrapper",
            ProxyChecker = "Proxy Checker"
        },
        SettingsMenu = new()
        {
            Tint = "Use arrows to select (or Escape/Backspace to go back)",
            ViewMode = "View mode",
            Language = "English",
            ProxyPath = "Proxy path",
            SaveDetails = "Save details"
        }
    };

    public class Manager
    {
        private readonly string _localePath = "./Localisation";
        private readonly object _fileLocker = new();

        public Base GetLocale(string localeFile)
        {
            localeFile = localeFile + ".json";
            string? localeString;
        
            try
            {
                localeString = File.ReadAllText(Path.Combine(_localePath, localeFile));
            }
            catch (Exception ex)
            {
                Program.Log.Error($"Error occured while trying to read {localeFile} file\n{ex}");
                Program.Log.Warning("Default localisation will be used");
                CreateDefaultLocale();
                return English;
            }

            var locale = JsonConvert.DeserializeObject<Base>(localeString);
            if (locale is null)
                Program.Log.Warning($"Loaded localisation file ({localeFile}) structure is different from Base structure. Default \"English\" localisation will be used");
            
            return locale ?? English;
        }

        public void Load() => CreateDefaultLocale();
        
        public void CreateDefaultLocale()
        {
            lock (_fileLocker)
            {
                Program.Log.Information("Checking default localisation");
                
                if (!Directory.Exists(_localePath)) 
                    Directory.CreateDirectory(_localePath);

                var localeFilePath = Path.Combine(_localePath, "English.json");
            
                if (File.Exists(localeFilePath)) 
                    return;

                Program.Log.Information("Creating default localisation");
                
                var defaultLocale = English;
                var defaultLocaleString = JsonConvert.SerializeObject(defaultLocale, Formatting.Indented);

                File.WriteAllText(localeFilePath, defaultLocaleString);
                
                Program.Log.Information("Localisation creation process done");
            }
        }
    }
}