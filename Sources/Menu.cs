#pragma warning disable CS0649

using Pastel;

public class Menu
{
    public class Main
    {
        public static void Show()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? Path = Settings.Read("Main", "LastPath");
            string Output = $" {"# MAIN MENU".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"Use keys for choosing (example - 1. is equal \"1\" button)"} \n\n {(Path is null || Path == "" ? $"{"1. Launch".Pastel(ConsoleColor.DarkGray)} {"[path not set]".Pastel(ConsoleColor.Red)}" : "1. Launch")}\n 2. Settings";
            Console.WriteLine(Output);

            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                var KeyName = keyInfo.Key;

                switch (KeyName)
                {
                    case ConsoleKey.D2:
                        Menu.Settings.Show();
                        return;
                    case ConsoleKey.D1:
                        if (Path is null || Path == "") break;
                        else
                        {
                            Launch.Start();
                            return;
                        };
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

        public static void SetSimplifiedView(bool Value)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            Settings.Write("Output", "SimplifiedView", Value.ToString());
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

            if (!Directory.Exists(NewPath))
            {
                Console.WriteLine(" ! New path is not exists or it is a path to a file\n ! Retrying in 2 seconds...".Pastel(ConsoleColor.Red));
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

        public static void ShowThreads()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? Threads = Settings.Read("Main", "Threads");
            string Output = $" {"# MAIN MENU > SETTINGS > THREADS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"No input - back to settings without any changes\n The number of threads determines how many tasks can run in parallel".Pastel(ConsoleColor.Gray)}\n\n ? Current Threads {$"[{(Threads is null || Threads == "" ? "not set" : Threads)}]".Pastel(System.Drawing.Color.GreenYellow)}\n $ New Threads: ";

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
                ShowThreads();
                return;
            }

            else
            {
                SetThreads(int.Parse(NewThreads));
                ShowThreads();
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

        public static void Show()
        {
            Utils.ClearAndShow();

            var Settings = new Utils.IniFile("Settings.ini");
            string? LastPath = Settings.Read("Main", "LastPath");
            string? ThreadsCount = Settings.Read("Main", "Threads");
            bool? WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            bool? SimplifiedView = Settings.Read("Output", "SimplifiedView") == "True";
            string Output = $" {"# MAIN MENU > SETTINGS".Pastel(System.Drawing.Color.OrangeRed)}\n\n {"P.S. Format (key|name|[value]).\n Use keyboard to select key which you want change, ESC - go to main menu\n If settings wasn't set or something not working properly, use \"Load default settings\"".Pastel(ConsoleColor.Gray)}\n\n 1. Path {$"[{(LastPath is null || LastPath == "" ? "not set" : LastPath)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 2. Threads {$"[{(ThreadsCount is null ? "not set" : ThreadsCount)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 3. Write types {$"[{(WriteTypes is null ? "not set" : WriteTypes)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 4. Simplified View {$"[{(SimplifiedView is null ? "not set" : SimplifiedView)}]".Pastel(System.Drawing.Color.GreenYellow)}\n 5. Load Default Settings";
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
                    case ConsoleKey.D1:
                        ShowPath();
                        return;
                    case ConsoleKey.D2:
                        ShowThreads();
                        return;
                    case ConsoleKey.D3:
                        ShowWriteTypes();
                        return;
                    case ConsoleKey.D4:
                        ShowSimplifiedView();
                        return;
                    case ConsoleKey.D5:
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
        public static void Start()
        {
            var Settings = new Utils.IniFile("Settings.ini");
            var filePath = Settings.Read("Main", "LastPath");

            if (!Directory.Exists(filePath))
            {
                if (filePath is null)
                    Console.WriteLine(" ! Path is NULL, try again");
                else
                    Console.WriteLine(" ! Folder not found...");
                
                return;
            }

            DateTime StartTime = DateTime.Now;

            string LogsPath = filePath;
            string ResultsPath = "./results";
            string RecordPath = $"{ResultsPath}/{StartTime:dd.MM.yyyy} ({StartTime:H-mm-ss})";
            Decryptor.Call(LogsPath, ResultsPath, RecordPath);
            Console.WriteLine("\n");
        }
    }
}