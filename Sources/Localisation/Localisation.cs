#pragma warning disable

using Multitale.Sources.Helpers;

namespace Multitale.Sources.Localisation;

public class Localisation
{
    public static Base DefaultLocale = new English();
    public static string DefaultLocaleName = DefaultLocale.GetType().Name;
    
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
    
    public class Manager : JsonManager<Base>
    {
        public const string LocalesPath = "./Localisation";
        
        public Manager() : base(LocalesPath, DefaultLocale, "locale") { }

        public static Base? GetLocaleFromFile(string localeFile) => GetFromFile(localeFile);

        public Base GetLocale(string localeName) => Get(localeName);

        public void Load() => base.Load(new Russian());
    }
}