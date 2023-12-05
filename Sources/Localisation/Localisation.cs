#pragma warning disable

using Multitale.Sources.Helpers;

namespace Multitale.Sources.Localisation;

public class Localisation
{
    public static Base DefaultLocale = new English();
    public static string DefaultLocaleName = DefaultLocale.GetType().Name;
    private static Dictionary<string, Base> _cache = new();
    
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
        public string InputTint { get; set; }
        public string Settings { get; set; }
        public string Language { get; set; }
        public string Theme { get; set; }
        public string ViewMode { get; set; }
        public string SaveDetails { get; set; }
        public string ProxyPath { get; set; }
        public string ProxyTimeout { get; set; }
        public string DataFilePath { get; set; }
        public string GoBack { get; set; }
        public string GoBackToInput { get; set; }
        public string Change { get; set; }
        public string Enable { get; set; }
        public string Disable { get; set; }
        public string Enabled { get; set; }
        public string Disabled { get; set; }
        public string InputValue { get; set; }
        public string InputDirectoryPath { get; set; }
        public string InputFilePath { get; set; }
        public string InputNumber { get; set; }
        public string DirectoryNotExists { get; set; }
        public string FileNotExists { get; set; }
        public string NotNumber { get; set; }
        public string NotSet { get; set; }
        public string CurrentValue { get; set; }
        public string Threads { get; set; }
    }

    public class LauncherMenu
    {
        public string Tint { get; set; }
        public string StartTint { get; set; }
        public string Launcher { get; set; }
        public string Decoder { get; set; }
        public string Fetcher { get; set; }
        public string ProxyScrapper { get; set; }
        public string ProxyChecker { get; set; }
        public string Start { get; set; }
        public string Settings { get; set; }
        public string GoBack { get; set; }
        public string ThreadsNotSet { get; set; }
        public string DataFileNotSet { get; set; }
        public string DataFileNotExists { get; set; }
        public string DataFileNothingParsed { get; set; }
        public string ProxyFileNotSet { get; set; }
        public string ProxyFileNotExists { get; set; }
        public string ProxyFileNothingParsed { get; set; }
        public string ProxyTimeoutNotSet { get; set; }
        public string ParsingDataFile { get; set; }
        public string ParsingProxyFile { get; set; }
        public string FromFile { get; set; }
        public string CollectedWallets { get; set; }
        public string CollectedProxy { get; set; }
        public string ValidatingProxy { get; set; }
        public string ValidatedProxy { get; set; }
        public string ValidatedProxyNothingParsed { get; set; }
        public string Fetching { get; set; }
        public string Balance { get; set; }
        public string Wallet { get; set; }
        public string Mnemonic { get; set; }
        public string PrivateKey { get; set; }
        public string TotalBalance { get; set; }
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
        
        public Manager() : base(LocalesPath, DefaultLocale, "locale", ref _cache) { }

        public static Base? GetLocaleFromFile(string localeFile) => GetFromFile(localeFile);

        public Base GetLocale(string localeName) => Get(localeName);

        public void Load() => base.Load(new Russian());
    }
}