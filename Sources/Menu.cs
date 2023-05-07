#pragma warning disable CS0649

using Pastel;

public class Menu
{
    public class Main
    {
        public static void Show()
        {
            Utils.ClearAndShow();
            Console.Title = "Multitale ~";
            
            string Output = $" {"# MAIN MENU".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"Use keys for choosing (example - 1. is equal \"1\" button)".Pastel(ConsoleColor.Gray)} \n\n 1. Launch\n 2. Settings";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D2:
                        Settings.Show();
                        return;
                    case ConsoleKey.D1:
                        Launch.Show();
                        return;
                    default:
                        break;
                }
            }
        }
    }

    public class Settings
    {
        public static void SetThreads(int ThreadsCount)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Main", "Threads", ThreadsCount.ToString());
        }

        public static void SetWriteTypes(bool Value)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Recorder", "WriteTypes", Value.ToString());
        }

        public static void SetPath(string Path)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Main", "LastPath", Path);
        }

        public static void SetProxiesPath(string Path)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Main", "ProxiesPath", Path);
        }

        public static void SetSimplifiedView(bool Value)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Output", "SimplifiedView", Value.ToString());
        }

        public static void SetLauncherFetcher(bool Value)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Launcher", "Fetcher", Value.ToString());
        }

        public static void SetFetcherThreads(int ThreadsCount)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Fetcher", "Threads", ThreadsCount.ToString());
        }

        public static void SetDecryptorThreads(int ThreadsCount)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Decryptor", "Threads", ThreadsCount.ToString());
        }

        public static void SetDecryptorFetch(bool Value)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Decryptor", "Fetch", Value.ToString());
        }

        public static void ShowPath()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? LastPath = Settings.Read("Main", "LastPath");
            string Output = $" {"# MAIN MENU > SETTINGS > PATH".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n The Path is the location of the folder that the program will work with".Pastel(ConsoleColor.Gray)}\n\n ? Current path {$"[{(LastPath is null || LastPath == "" ? "not set" : LastPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n $ New path: ";

            Console.CursorVisible = true;
            Console.Write(Output);
            string? NewPath = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewPath == "")
            {
                Show();
                return;
            }

            if (!Directory.Exists(NewPath) && !File.Exists(NewPath))
            {
                Console.WriteLine(" ! New path is not exists\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
                Thread.Sleep(2000);
                ShowPath();
                return;
            }

            else
            {
                SetPath(NewPath);
                ShowPath();
                return;
            }
        }

        public static void ShowProxiesPath()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? ProxiesPath = Settings.Read("Main", "ProxiesPath");
            string Output = $" {"# MAIN MENU > SETTINGS > PROXIES PATH".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n The Proxies path is the location of the file containing proxies that the program will use to work with".Pastel(ConsoleColor.Gray)}\n\n ? Current path {$"[{(ProxiesPath is null || ProxiesPath == "" ? "not set" : ProxiesPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n $ New path: ";

            Console.CursorVisible = true;
            Console.Write(Output);
            string? NewPath = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewPath == "")
            {
                Show();
                return;
            }

            if (!File.Exists(NewPath))
            {
                Console.WriteLine(" ! New proxies path is not a file or not exists\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
                Thread.Sleep(2000);
                ShowProxiesPath();
                return;
            }

            if (Utils.GrabProxies(NewPath) is null)
            {
                Console.WriteLine(" ! New proxies path is not contains any proxies\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
                Thread.Sleep(2000);
                ShowProxiesPath();
                return;
            }

            else
            {
                SetProxiesPath(NewPath);
                ShowProxiesPath();
                return;
            }
        }

        public static void ShowWriteTypes()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            bool? WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            string Output = $" {"# MAIN MENU > SETTINGS > WRITE TYPES".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"ESC - back to settings without any changes\n Write Types is saving information by separate types where this function supported".Pastel(ConsoleColor.Gray)}\n\n ? Current state {$"[{(WriteTypes is null ? "not set" : WriteTypes)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 1. Enable\n 2. Disable";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D1:
                        SetWriteTypes(true);
                        ShowWriteTypes();
                        return;
                    case ConsoleKey.D2:
                        SetWriteTypes(false);
                        ShowWriteTypes();
                        return;
                    case ConsoleKey.Escape:
                        Show();
                        return;
                    case ConsoleKey.Backspace:
                        Show();
                        return;
                    default:
                        break;
                }
            }
        }

        public static void ShowSimplifiedView()
        {

            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            bool? SimplifiedView = Settings.Read("Output", "SimplifiedView") == "True";
            string Output = $" {"# MAIN MENU > SETTINGS > SIMPLIFIED VIEW".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"ESC - back to settings without any changes\n Simple View is Simplified display of results in the console".Pastel(ConsoleColor.Gray)}\n\n ? Current state {$"[{(SimplifiedView is null ? "not set" : SimplifiedView)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 1. Enable\n 2. Disable";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D1:
                        SetSimplifiedView(true);
                        ShowSimplifiedView();
                        return;
                    case ConsoleKey.D2:
                        SetSimplifiedView(false);
                        ShowSimplifiedView();
                        return;
                    case ConsoleKey.Escape:
                        Show();
                        return;
                    case ConsoleKey.Backspace:
                        Show();
                        return;
                    default:
                        break;
                }
            }
        }

        public static void ShowReset()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            var AllSettings = Settings.ReadAll();
            var DefaultSettings = Utils.GetDefaults();
            if (AllSettings is null) return;
            string Output = $" {"# MAIN MENU > SETTINGS > LOAD DEFAULT SETTINGS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n Load default settings is the function to load the default values for the settings file".Pastel(ConsoleColor.Gray)}\n\n";

            AllSettings.RemoveAll(el => el.Section == "Main" && (el.Key == "Version" || el.Key == "LastVisit"));
            DefaultSettings.RemoveAll(el => el.Section == "Main" && (el.Key == "Version" || el.Key == "LastVisit"));
            var DifferentData = Utils.GetDifferentIniData(AllSettings, DefaultSettings);

            Console.WriteLine(Output + string.Format(" {0,-18} {1,-18} {2}", "PARAMETER", "CURRENT", "DEFAULT").Pastel(System.Drawing.Color.Orange));

            if (DifferentData.Count < 1)
            {
                foreach (var Element in AllSettings)
                {
                    var OldElement = AllSettings.Where(item => item.Section == Element.Section && item.Key == Element.Key).ToList()[0];
                    Console.WriteLine(string.Format(" {0,-18} {1,-18} {2}", OldElement.Key, OldElement.Value, Element.Value).Pastel(System.Drawing.Color.White));
                }

                Console.WriteLine("\n ? All values are equal to the default values. Press ESC to go back");

                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    var KeyName = keyInfo.Key;

                    switch (KeyName)
                    {
                        case ConsoleKey.Escape:
                            Show();
                            return;
                        default:
                            break;
                    }
                }
            }

            foreach (var Element in DifferentData)
            {
                if (Element.Section == "Main" && Element.Key == "LastVisit") continue;
                var OldElement = AllSettings.Where(item => item.Section == Element.Section && item.Key == Element.Key).ToList()[0];
                string DefaultValue = Element.Value;
                string CurrentValue = OldElement.Value;

                if (CurrentValue.Length > 18)
                    CurrentValue = CurrentValue[..15] + "..";
                if (DefaultValue.Length > 18)
                    DefaultValue = DefaultValue[..15] + "..";

                Console.WriteLine(string.Format(" {0,-18} {1,-18} {2}", OldElement.Key, CurrentValue, DefaultValue).Pastel(System.Drawing.Color.White));
            }

            Console.CursorVisible = true;
            Console.Write("\n Type \"RESET\" if you really want to set defaults: ");
            

            string? Input = Console.ReadLine();
            if (Input == "" || Input is null) { Show(); return; };
            
            if (Input.ToUpper() == "RESET")
            {
                Utils.LoadDefaults();
                ShowReset();
            }
        }

        public static void ShowFetcherThreads()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? Threads = Settings.Read("Fetcher", "Threads");
            string Output = $" {"# MAIN MENU > SETTINGS > FETCHER THREADS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n The number of threads determines how many tasks can run in parallel".Pastel(ConsoleColor.Gray)}\n\n ? Current Threads {$"[{(Threads is null || Threads == "" ? "not set" : Threads)}]".Pastel(System.Drawing.Color.GreenYellow)}\n $ New Threads: ";

            Console.CursorVisible = true;
            Console.Write(Output);
            string? NewThreads = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewThreads == "")
            {
                Show();
                return;
            }

            if (NewThreads is null || !int.TryParse(NewThreads, out _) || int.Parse(NewThreads) == 0)
            {
                Console.WriteLine(" ! New threads is not a number or it is has zero value\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
                Thread.Sleep(2000);
                ShowFetcherThreads();
                return;
            }

            else
            {
                SetFetcherThreads(int.Parse(NewThreads));
                ShowFetcherThreads();
                return;
            }
        }

        public static void ShowDecryptorThreads()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? Threads = Settings.Read("Decryptor", "Threads");
            string Output = $" {"# MAIN MENU > SETTINGS > DECRYPTOR THREADS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n The number of threads determines how many tasks can run in parallel".Pastel(ConsoleColor.Gray)}\n\n ? Current Threads {$"[{(Threads is null || Threads == "" ? "not set" : Threads)}]".Pastel(System.Drawing.Color.GreenYellow)}\n $ New Threads: ";

            Console.CursorVisible = true;
            Console.Write(Output);
            string? NewThreads = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewThreads == "")
            {
                Show();
                return;
            }

            if (NewThreads is null || !int.TryParse(NewThreads, out _) || int.Parse(NewThreads) == 0)
            {
                Console.WriteLine(" ! New threads is not a number or it is has zero value\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
                Thread.Sleep(2000);
                ShowDecryptorThreads();
                return;
            }

            else
            {
                SetDecryptorThreads(int.Parse(NewThreads));
                ShowDecryptorThreads();
                return;
            }
        }

        public static void ShowDecryptorFetch()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            bool? DecryptorFetch = Settings.Read("Decryptor", "Fetch") == "True";
            string Output = $" {"# MAIN MENU > SETTINGS > DECRYPTOR FETCH".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"ESC - back to settings without any changes\n Fetcher is checking parsed decrypted data for balances, pools, etc.".Pastel(ConsoleColor.Gray)}\n\n ? Current state {$"[{(DecryptorFetch is null ? "not set" : DecryptorFetch)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 1. Enable\n 2. Disable";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D1:
                        SetDecryptorFetch(true);
                        ShowDecryptorFetch();
                        return;
                    case ConsoleKey.D2:
                        SetDecryptorFetch(false);
                        ShowDecryptorFetch();
                        return;
                    case ConsoleKey.Escape:
                        Show();
                        return;
                    case ConsoleKey.Backspace:
                        Show();
                        return;
                    default:
                        break;
                }
            }
        }

        public static void Show()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? LastPath = Settings.Read("Main", "LastPath");
            string? ProxiesPath = Settings.Read("Main", "ProxiesPath");
            string? FetcherThreadsCount = Settings.Read("Fetcher", "Threads");
            string? DecryptorThreadsCount = Settings.Read("Decryptor", "Threads");
            bool? DecryptorFetch = Settings.Read("Decryptor", "Fetch") == "True";
            bool? WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            bool? SimplifiedView = Settings.Read("Output", "SimplifiedView") == "True";

            string Output = $" {"# MAIN MENU > SETTINGS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"P.S. Format (key|name|[value]).\n Use keyboard to select key which you want change, ESC - go to main menu\n If settings wasn't set or something not working properly, use \"Load default settings\"".Pastel(ConsoleColor.Gray)}\n\n 1. Working path {$"[{(LastPath is null || LastPath == "" ? "not set" : LastPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 2. Proxies path {$"[{(ProxiesPath is null || ProxiesPath == "" ? "not set" : ProxiesPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 3. Decryptor fetching {$"[{(DecryptorFetch is null ? "not set" : DecryptorFetch)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 4. Decryptor threads {$"[{(DecryptorThreadsCount is null ? "not set" : DecryptorThreadsCount)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 5. Fetcher threads {$"[{(FetcherThreadsCount is null ? "not set" : FetcherThreadsCount)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 6. Write types {$"[{(WriteTypes is null ? "not set" : WriteTypes)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 7. Simplified view {$"[{(SimplifiedView is null ? "not set" : SimplifiedView)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 8. Load default settings";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.Escape:
                        Main.Show();
                        return;
                    case ConsoleKey.Backspace:
                        Main.Show();
                        return;
                    case ConsoleKey.D1:
                        ShowPath();
                        return;
                    case ConsoleKey.D2:
                        ShowProxiesPath();
                        return;
                    case ConsoleKey.D3:
                        ShowDecryptorFetch();
                        return;
                    case ConsoleKey.D4:
                        ShowDecryptorThreads();
                        return;
                    case ConsoleKey.D5:
                        ShowFetcherThreads();
                        return;
                    case ConsoleKey.D6:
                        ShowWriteTypes();
                        return;
                    case ConsoleKey.D7:
                        ShowSimplifiedView();
                        return;
                    case ConsoleKey.D8:
                        ShowReset();
                        return;
                    default:
                        break;
                }
            }
        }
    }
    
    public class Launch
    {
        public static void Show()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            var Path = Settings.Read("Main", "LastPath");
            if (Path == "") Path = null;
            var ProxiesPath = Settings.Read("Main", "ProxiesPath");
            if (ProxiesPath == "") ProxiesPath = null;
            bool IsContainsProxies = false;
            bool IsContainsDecryptedData = false;
            bool IsPathFile = false;
            bool IsProxiesPathFile = false;

            if (ProxiesPath is not null || ProxiesPath != "")
                IsProxiesPathFile = File.Exists(ProxiesPath);
            if (ProxiesPath is not null && IsProxiesPathFile)
                IsContainsProxies = Utils.GrabProxies(ProxiesPath) is not null;

            if (Path is not null || Path != "")
                IsPathFile = File.Exists(Path);
            if (Path is not null && IsPathFile)
                IsContainsDecryptedData = Utils.GrabPrivateKeys(Path)?.Count > 0 || Utils.GrabMnemonics(Path)?.Count > 0;

            string Output = $" {"# MAIN MENU > LAUNCHER".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"Use keys for choosing (example - 1. is equal \"1\" button)".Pastel(ConsoleColor.Gray)}\n\n {(Path is null ? $"{"1. Decryptor".Pastel(ConsoleColor.DarkGray)} {"[path not set]".Pastel(ConsoleColor.Red)}" : IsPathFile is true ? $"{"1. Decryptor".Pastel(ConsoleColor.DarkGray)} {"[path is not a directory]".Pastel(ConsoleColor.Red)}" : "1. Decryptor")}\n {(Path is null ? $"{"2. Fetcher".Pastel(ConsoleColor.DarkGray)} {"[path not set]".Pastel(ConsoleColor.Red)}" : IsPathFile is false ? $"{"2. Fetcher".Pastel(ConsoleColor.DarkGray)} {"[path is not a file]".Pastel(ConsoleColor.Red)}" : IsContainsDecryptedData is false ? $"{"2. Fetcher".Pastel(ConsoleColor.DarkGray)} {"[file is not contains any mnemonics or keys]".Pastel(ConsoleColor.Red)}" : IsContainsProxies is false ? $"{"2. Fetcher".Pastel(ConsoleColor.DarkGray)} {"[proxies path is not contains any proxies]".Pastel(ConsoleColor.Red)}" : "2. Fetcher")}\n 3. Proxy Scrapper\n {(ProxiesPath is null ? $"{"4. Proxy Validator".Pastel(ConsoleColor.DarkGray)} {"[path not set]".Pastel(ConsoleColor.Red)}" : IsProxiesPathFile is false ? $"{"4. Proxy Validator".Pastel(ConsoleColor.DarkGray)} {"[path is not a file]".Pastel(ConsoleColor.Red)}" : IsContainsProxies is false ? $"{"4. Proxy Validator".Pastel(ConsoleColor.DarkGray)} {"[file is not contains any proxies]".Pastel(ConsoleColor.Red)}" : "4. Proxy Validator")}";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D1:
                        if (Path is null || Path == "" || IsPathFile is true)
                            break;
                        else
                        {
                            Start(Structures.Functions.Decryptor);
                            return;
                        }
                    case ConsoleKey.D2:
                        if (Path is null || Path == "" || IsPathFile is false || IsContainsDecryptedData is false || IsContainsProxies is false)
                            break;
                        else
                        {
                            Start(Structures.Functions.Fetcher);
                            return;
                        }
                    case ConsoleKey.D3:
                        Start(Structures.Functions.ProxyScrapper);
                        return;
                    case ConsoleKey.D4:
                        if (Path is null || Path == "" || IsProxiesPathFile is false || IsContainsProxies is false)
                            break;
                        else
                        {
                            Start(Structures.Functions.ProxyValidator);
                            return;
                        }
                    case ConsoleKey.Escape:
                        Main.Show();
                        return;
                    case ConsoleKey.Backspace:
                        Main.Show();
                        return;
                    default:
                        break;
                }
            }
        }

        public static void Start(Structures.Functions Function)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            var FilePath = Settings.Read("Main", "LastPath");
            if (FilePath == "") FilePath = null;
            bool IsFetcher = Settings.Read("Decryptor", "Fetch") == "True";
            var ProxiesPath = Settings.Read("Main", "ProxiesPath");
            if (ProxiesPath == "") ProxiesPath = null;
            bool IsContainsProxies = false;

            if (ProxiesPath is not null)
                IsContainsProxies = Utils.GrabProxies(ProxiesPath) is not null;

            Utils.ClearAndShow();
            Console.WriteLine(" # MAIN MENU > LAUNCH".Pastel(System.Drawing.Color.OrangeRed) + "\n");

            if (!Directory.Exists(FilePath) && !File.Exists(FilePath))
            {
                if (FilePath is null)
                    Console.WriteLine(" ! Path is NULL, try again");
                else
                    Console.WriteLine(" ! Path not found...");
                return;
            }

            if (ProxiesPath is not null && !File.Exists(ProxiesPath))
            {
                if (Directory.Exists(ProxiesPath))
                    Console.WriteLine(" ! Proxies path can't be a directory");
                else
                    Console.WriteLine(" ! Path is not exists");
                return;
            }

            DateTime StartTime = DateTime.Now;

            string LogsPath = FilePath;
            string ResultsPath = "./Results";
            if (!Directory.Exists(ResultsPath))
                Utils.TryCreateDirectory(ResultsPath);

            string RecordPath = $"{ResultsPath}/{StartTime:yyyy.MM.dd} ({StartTime:H-mm-ss})";

            List<Structures.Vault.Secret>? Secrets = null;
            List<Structures.Balances.Total>? Balances = null;
            List<Structures.Proxy>? Proxies = null;

            if (Function is Structures.Functions.Decryptor) 
            {
                Secrets = Decryptor.Call(LogsPath, RecordPath);
                if (Secrets.Count == 0) Secrets = null;
            }

            if (Function is Structures.Functions.Fetcher || (Function is Structures.Functions.Decryptor && IsFetcher))
            {
                if (IsContainsProxies is true)
                {
                    Balances = Fetchers.Call(FilePath, RecordPath, Secrets, Function is Structures.Functions.Decryptor && IsFetcher);
                    if (Balances.Count == 0) Balances = null;
                }
                else
                {
                    Console.WriteLine(" ! Fetcher can't be launched because no any proxies found in proxies path");
                }
            }

            if (Function is Structures.Functions.ProxyScrapper)
            {
                Proxies = Utils.ProxyScrapper.CallScrapper(RecordPath);
            }

            if (Function is Structures.Functions.ProxyValidator)
            {
                Proxies = Utils.GrabProxies(ProxiesPath);

                if (Proxies is not null)
                    Proxies = Utils.ProxyScrapper.CallValidator(RecordPath, Proxies);
            }

            Console.WriteLine("\n # Process ended. Press any key to return to main menu");
            Console.ReadKey(true);
            Main.Show();
        }
    }
}