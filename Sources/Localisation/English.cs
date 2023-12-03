namespace Multitale.Sources.Localisation;

public class English : Localisation.Base
{
    public English()
    {
        Alias = "English";
        HomeMenu = new Localisation.HomeMenu
        {
            Tint = "Use arrows to select and enter or space to confirm",
            Home = "Homepage",
            About = "About",
            Launcher = "Launcher",
            Settings = "Settings",
            Donate = "Donate"
        };
        AboutMenu = new Localisation.AboutMenu
        {
            Tint = "Use arrows to select and enter or space to confirm",
            About = "About",
            Developers = "Developers",
            AboutText =
                "I decided to make a project that could interest both me and people who know what it is for. " +
                "I wanted to try to make something that could be redesigned for myself and supplemented by any developer who would like to try himself in this field.",
            DevelopersText = "cradles, saykowa",
            Links = "Links",
            LinksText = "https://github.com/cryingincradles/Multitale" + "\n" +
                        "https://t.me/multitale" + "\n" +
                        "https://zelenka.guru/threads/5167800",
            GoBack = "Go back"
        };
        DonateMenu = new Localisation.DonateMenu
        {
            Tint = "Use arrows to select and enter or space to confirm",
            Donate = "Donate",
            DonateText =
                "Multitale is my biggest development practice, to which I spend a huge amount of my time. " +
                "So, if you want the project to continue to be supported, updated, and supplemented, you can leave your donation using the available requisites below",
            Requisites = "Requisites",
            RequisitesText = "ETH/BSC -> 0x375c1A4CcC41FcB2d35122aDDA008A8ecD384333" + "\n" +
                             "BTC -> bc1ql3thytsud4x9nulym3xkpmppv5eh8cj84tqsnp" + "\n" +
                             "LTC -> LaX2ZRgDwAWqsYhHLvZDc3xYz1sA2LEFuM" + "\n" +
                             "USDT -> TH2n3dQ8Jd2mzvoGatkpBPz5CHRhNTmnan" + "\n" +
                             "SOL -> EJWQbh4T43uV8mxkcbyLXCthG6CafqeyQBrvLMpHfSm2"
        };
        LauncherMenu = new Localisation.LauncherMenu
        {
            Tint = "Use arrows to select and enter or space to confirm",
            StartTint = "Please, be patient and wait until process will be completed",
            Start = "Start",
            Settings = "Settings",
            GoBack = "Go back",
            Launcher = "Launcher",
            Decoder = "Decoder",
            Fetcher = "Fetcher",
            ThreadsNotSet = "Threads amount is not set in the settings",
            DataFileNotSet = "Data file is not set in the settings",
            DataFileNotExists = "Data file is not exists",
            DataFileNothingParsed = "No one line was parsed from data file",
            ProxyFileNotSet = "The proxy file is not set in the settings",
            ProxyFileNotExists = "Proxy file does not exist or was not found",
            ProxyFileNothingParsed = "No any proxy from the specified file has been parsed",
            ProxyTimeoutNotSet = "Proxy timeout not set in the settings",
            ProxyScrapper = "Proxy Scrapper",
            ProxyChecker = "Proxy Checker",
            ParsingDataFile = "Parsing data-file lines",
            ParsingProxyFile = "Collecting proxy",
            FromFile = "From file",
            CollectedWallets = "Wallets collected",
            CollectedProxy = "Proxy collected",
            ValidatingProxy = "Validating proxy",
            ValidatedProxy = "Valid proxy count",
            ValidatedProxyNothingParsed = "No any valid proxy was parsed",
            Fetching = "Fetching wallets",
            Balance = "Balance",
            Wallet = "Wallet",
            Mnemonic = "Mnemonic",
            PrivateKey = "Key"
        };
        SettingsMenu = new Localisation.SettingsMenu
        {
            Tint = "Use arrows to select and enter or space to confirm",
            InputTint = "Leave the input field blank and press enter to go back",
            Settings = "Settings",
            ViewMode = "View mode",
            Language = "Language",
            Theme = "Theme",
            ProxyPath = "Proxy path",
            ProxyTimeout = "Proxy timeout",
            DataFilePath = "Data file path",
            SaveDetails = "Save details",
            GoBack = "Go back",
            GoBackToInput = "Go back to input",
            Change = "Change",
            Enable = "Enable",
            Disable = "Disable",
            Enabled = "Enabled",
            Disabled = "Disabled",
            InputValue = "Enter a value",
            InputNumber = "Enter a number",
            InputDirectoryPath = "Enter folder path",
            InputFilePath = "Enter file path",
            DirectoryNotExists = "The specified folder does not exist or was not found",
            FileNotExists = "The specified file does not exist or was not found",
            NotNumber = "The value entered is not numeric",
            NotSet = "Not set",
            CurrentValue = "Current value",
            Threads = "Threads"
        };
    }
}