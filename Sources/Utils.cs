using PuppeteerSharp;
using System.Text;
using System.Text.RegularExpressions;
using Leaf.xNet;
using static Structures;
using static Structures.Vault;
using Spectre.Console;

public partial class Utils
{
    public class IniFile
    {
        private string filePath;

        public IniFile(string filePath)
        {
            this.filePath = filePath;
            if (!File.Exists(filePath)) File.Create(filePath).Close();
        }

        private object _fileLock = new();

        public void Write(string section, string key, string value)
        {
            string line = $"{key}={value}";
            string sectionHeader = $"[{section}]";

            lock (_fileLock)
            {
                string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

                bool sectionFound = false;
                bool keyFound = false;

                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                    {
                        if (lines[i][1..^1] == section)
                        {
                            sectionFound = true;
                        }
                        else
                        {
                            sectionFound = false;
                        }
                    }
                    else if (sectionFound && lines[i].Contains("="))
                    {
                        string[] parts = lines[i].Split('=');
                        if (parts[0] == key)
                        {
                            lines[i] = line;
                            keyFound = true;
                            break;
                        }
                    }
                }

                if (!keyFound)
                {
                    if (!sectionFound)
                    {
                        File.AppendAllText(filePath, $"\r\n{sectionHeader}\r\n{line}", Encoding.UTF8);
                    }
                    else
                    {
                        int insertIndex = Array.IndexOf(lines, sectionHeader) + 1;
                        Array.Resize(ref lines, lines.Length + 1);
                        Array.Copy(lines, insertIndex, lines, insertIndex + 1, lines.Length - insertIndex - 1);
                        lines[insertIndex] = line;
                        File.WriteAllLines(filePath, lines, Encoding.UTF8);
                    }
                }
                else
                {
                    File.WriteAllLines(filePath, lines, Encoding.UTF8);
                }
            }
        }

        public string? Read(string section, string key)
        {
            string? defaultValue = null;
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            bool sectionFound = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (line[1..^1] == section)
                    {
                        sectionFound = true;
                    }
                    else
                    {
                        sectionFound = false;
                    }
                }
                else if (sectionFound && line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    if (parts[0] == key)
                    {
                        return parts[1];
                    }
                }
            }

            return defaultValue;
        }

        public List<IniData>? ReadSection(string section)
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<IniData>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    if (trimmedLine[1..^1] == section)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";"))
                {
                    var index = trimmedLine.IndexOf('=');

                    if (index != -1)
                    {
                        var key = trimmedLine[..index].Trim();
                        var value = trimmedLine[(index + 1)..].Trim();

                        result.Add(new()
                        {
                            Section = section,
                            Key = key,
                            Value = value
                        });
                    }
                }
            }

            if (result.Count == 0)
                return null;
            else
                return result;
        }

        public List<IniData>? ReadAll()
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<IniData>();
            var section = string.Empty;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    section = trimmedLine[1..^1];
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";") && section != string.Empty)
                {
                    var index = trimmedLine.IndexOf('=');

                    if (index != -1)
                    {
                        var key = trimmedLine[..index].Trim();
                        var value = trimmedLine[(index + 1)..].Trim();

                        result.Add(new()
                        {
                            Section = section,
                            Key = key,
                            Value = value
                        });
                    }
                }
            }

            if (result.Count == 0)
                return null;
            else
                return result;
        }

        public void RewriteAll(List<IniData> data)
        {
            var lines = new List<string>();
            var currentSection = string.Empty;

            foreach (var item in data)
            {
                if (item.Section != currentSection)
                {
                    if (currentSection != string.Empty)
                    {
                        lines.Add("");
                    }

                    lines.Add($"[{item.Section}]");
                    currentSection = item.Section;
                }

                lines.Add($"{item.Key}={item.Value}");
            }

            File.WriteAllLines(filePath, lines);
        }

        public bool IsEmpty()
        {
            return !File.ReadLines(filePath).Any(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(";"));
        }
    }

    public class Node
    {
        public static List<IJSHandle>? GetChildNodes(object Handled)
        {
            IJSHandle? PortofolioJSHandle = null;

            if (Handled is IJSHandle jsHandle)
            {
                PortofolioJSHandle = jsHandle.GetPropertyAsync("childNodes").GetAwaiter().GetResult();
            }

            else if (Handled is IElementHandle elHandle)
            {
                PortofolioJSHandle = elHandle.GetPropertyAsync("childNodes").GetAwaiter().GetResult();
            }

            else
            {
                throw new Exception("GetChildNodes(): Unsupported type of handled object");
            }

            if (PortofolioJSHandle is null) return null;

            var HandledChildNodes = (PortofolioJSHandle.EvaluateFunctionHandleAsync("nodes => Array.from(nodes)").GetAwaiter().GetResult()).GetPropertiesAsync().GetAwaiter().GetResult();
            if (HandledChildNodes is null) return null;
            return HandledChildNodes.Where(element => element.Key != "length").Select(element => element.Value).ToList();
        }

        public static IJSHandle? FindByClass(List<IJSHandle> HandledNodes, string IClassName)
        {
            IJSHandle? FoundedNode = HandledNodes.FirstOrDefault(node =>
            {
                var ClassName = node.GetPropertyAsync("className").GetAwaiter().GetResult()?.JsonValueAsync()?.GetAwaiter().GetResult()?.ToString();
                if (ClassName is null) return false;
                if (ClassName.Contains(IClassName))
                    return true;
                else
                    return false;
            });
            if (FoundedNode is null) return null;
            return FoundedNode;
        }
    }

    public class CustomSpinners
    {
        public sealed class Lines : Spinner
        {
            public override TimeSpan Interval => TimeSpan.FromMilliseconds(100);

            public override bool IsUnicode => false;

            public override IReadOnlyList<string> Frames =>
                new List<string>
                {
                " |    ", " ||   ", " ||| ", "  |||", "   ||", "    |", "     ", "     "
                };
        }
    }

    private static List<IniData> Defaults = new()
    {
            new() { Section = "Main", Key = "Version", Value = Program.CurrentRelease },
            new() { Section = "Main", Key = "LastPath", Value = "" },
            new() { Section = "Main", Key = "ProxiesPath", Value = "" },
            new() { Section = "Decryptor", Key = "Fetch", Value = "True" },
            new() { Section = "Decryptor", Key = "Threads", Value = "40" },
            new() { Section = "Fetcher", Key = "Threads", Value = "40" },
            new() { Section = "Fetcher", Key = "Method", Value = "Web" },
            new() { Section = "Recorder", Key = "WriteTypes", Value = "True" },
            new() { Section = "Output", Key = "SimplifiedView", Value = "False" }
    };

    private static List<string> CommonPasswords = new()
    {
        "password",
        "12345678",
        "12312312",
        "123123123",
        "qwertyui",
        "iloveyou",
        "admin123",
        "letmein69",
        "dragon69",
        "welcome",
        "sunshine",
        "monkey12",
        "password1",
        "superman",
        "shadow69",
        "master12",
        "mustang1",
        "football",
        "starwars",
        "whatever",
        "chocolate",
        "samantha",
        "thunder1",
        "baseball",
        "soccer12",
        "charlie1",
        "jennifer",
        "liverpool",
        "courage1",
        "harrypot",
        "william1",
        "princess",
        "jesus123",
        "blink182",
        "killer69",
        "scoobydo",
        "qazwsxed",
        "flower12",
        "abcdefg1",
        "daniel12",
        "michael1",
        "newyork1"
    };

    public static List<IniData> GetDifferentIniData(List<IniData> OldData, List<IniData> NewData)
    {
        var DifferentData = new List<IniData>();

        foreach (IniData OldIniElement in OldData)
        {
            foreach (IniData NewIniElement in NewData)
            {
                if (NewIniElement.Section == OldIniElement.Section &&
                    NewIniElement.Key == OldIniElement.Key &&
                    NewIniElement.Value != OldIniElement.Value)
                {
                    DifferentData.Add(NewIniElement);
                }
            }
        }

        return DifferentData;
    }

    public static void ClearAndShow()
    {
        Console.CursorVisible = false;
        AnsiConsole.Clear();
        Program.ShowLogo();

        //Console.Write("\u001b[2J\u001b[3J");
        //Console.Clear();
        //
    }

    public static void LoadDefaults()
    {
        var Settings = new IniFile("Settings.ini");
        Settings.RewriteAll(Defaults);
    }

    public static List<IniData> GetDefaults()
    {
        return Defaults;
    }

    public static bool IsInternetAvailable()
    {
        using var Request = new HttpRequest();
        Request.ConnectTimeout = 5000;
        Request.IgnoreProtocolErrors = true;
        try
        {
            var response = Request.Get("https://example.com");
            return response.IsOK;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static List<string>? GetLines(string path)
    {
        var PasswordsData = File.ReadAllText(path);
        var PasswordsLines = new List<string>();
        var Regex = new System.Text.RegularExpressions.Regex("(: )(.*)");
        var Matches = Regex.Matches(PasswordsData);
        if (Matches.Count < 1) return null;

        foreach (Match Match in Matches)
        {
            if (Match is not null) PasswordsLines.Add(Match.Groups[2].Value.Trim());
        }

        return PasswordsLines.ToHashSet().ToList();
    }

    public static ProxyClient GetProxyClient(Proxy Proxy)
    {
        ProxyClient PClient = Proxy.Type switch
        {
            Structures.ProxyType s when s is Structures.ProxyType.HTTP => Proxy.Login is not null && Proxy.Password is not null ? new HttpProxyClient(Proxy.IP, Proxy.Port, Proxy.Login, Proxy.Password) : new HttpProxyClient(Proxy.IP, Proxy.Port),
            Structures.ProxyType s when s is Structures.ProxyType.HTTPS => Proxy.Login is not null && Proxy.Password is not null ? new HttpProxyClient(Proxy.IP, Proxy.Port, Proxy.Login, Proxy.Password) : new HttpProxyClient(Proxy.IP, Proxy.Port),
            Structures.ProxyType s when s is Structures.ProxyType.SOCKS4 => new Socks4ProxyClient(Proxy.IP, Proxy.Port),
            Structures.ProxyType s when s is Structures.ProxyType.SOCKS5 => Proxy.Login is not null && Proxy.Password is not null ? new Socks5ProxyClient(Proxy.IP, Proxy.Port, Proxy.Login, Proxy.Password) : new Socks5ProxyClient(Proxy.IP, Proxy.Port),
            _ => HttpProxyClient.Parse($"{Proxy.IP}:{Proxy.Port}"),
        };

        return PClient;
    }

    public static List<string>? FindPasswords(string root_path, string? path = null)
    {
        List<string> Passwords = new();

        if (path == root_path) return null;
        path ??= root_path;

        var Files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories)
            .Where(el => el.Contains("assword"))
            .ToList();
        if (Files.Count < 1) return FindPasswords(root_path, Path.GetDirectoryName(path));

        foreach (var File in Files)
        {
            var Lines = GetLines(File);
            if (Lines is null) continue;

            Lines.ForEach(el => Passwords.Add(el));
        }

        return Passwords;
    }

    public static List<Proxy>? GrabProxies(string path)
    {
        List<Proxy> Proxies = new();
        var FileData = File.ReadAllText(path);
        var ProxiesRegex = new System.Text.RegularExpressions.Regex("(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}):(\\d{1,5})(\\:(\\w*):(\\w*))?", RegexOptions.Multiline);
        var ProxiesMatches = ProxiesRegex.Matches(FileData);

        if (ProxiesMatches.Count > 0)
        {
            foreach (Match ProxyMatch in ProxiesMatches)
            {
                var SplittedData = ProxyMatch.Value.Split(":");
                if (SplittedData.Length < 2) continue;

                var Proxy = new Proxy()
                {
                    IP = SplittedData[0],
                    Port = int.Parse(SplittedData[1]),
                    Login = SplittedData.Length > 2 ? SplittedData[2] : null,
                    Password = SplittedData.Length > 3 ? SplittedData[3] : null
                };

                if (Proxy.Port == 80 || Proxy.Port == 8080 || Proxy.Port == 3128)
                    Proxy.Type = Structures.ProxyType.HTTP;

                else if (Proxy.Port == 443 || Proxy.Port == 8443 || Proxy.Port == 563)
                    Proxy.Type = Structures.ProxyType.HTTPS;

                else
                    Proxy.Type = Structures.ProxyType.SOCKS4;

                Proxies.Add(Proxy);
            }

            return Proxies.Distinct().ToList();
        }

        else return null;
    }

    public static List<Proxy>? GrabProxiesFromSettings()
    {
        var Settings = new IniFile("Settings.ini");
        var ProxiesPath = Settings.Read("Main", "ProxiesPath");
        if (ProxiesPath is null) return null;

        var Proxies = GrabProxies(ProxiesPath);
        if (Proxies is null || Proxies?.Count == 0) return null;

        else return Proxies;
    }

    public static List<StringWallet>? GrabMnemonics(string path)
    {
        var SMnemonics = new List<StringWallet>();

        var FText = File.ReadAllText(path);
        if (FText is null) return null;
        FText = string.Join("\n", FText.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 1).ToList());

        var MnemonicExp = new string("([a-z]+ ){11}[a-z]+$|^([a-z]+ ){14}[a-z]+$|^([a-z]+ ){17}[a-z]+$|^([a-z]+ ){20}[a-z]+$|^([a-z]+ ){23}[a-z]+");
        var MnemonicREXP = new System.Text.RegularExpressions.Regex(MnemonicExp, RegexOptions.Multiline);

        MatchCollection MnemonicMatches = MnemonicREXP.Matches(FText);

        if (MnemonicMatches?.Count > 0)
            MnemonicMatches.ToList().ForEach(match => SMnemonics.Add(new() { Type = StringWalletTypes.Mnemonic, Value = match.Value }));

        if (SMnemonics.Count == 0) return null;
        return SMnemonics;
    }

    public static List<StringWallet>? GrabPrivateKeys(string path)
    {
        var SPrivateKeys = new List<StringWallet>();

        var FText = File.ReadAllText(path);
        if (FText is null) return null;
        FText = string.Join("\n", FText.Split('\n').Select(line => line.Trim()).Where(line => line.Length > 1).ToList());

        var PrivateKeyExp = new string("^(0x)?[0-9a-fA-F]{64}$");
        var PrivateKeyREXP = new System.Text.RegularExpressions.Regex(PrivateKeyExp, RegexOptions.Multiline);

        MatchCollection PrivateKeysMatches = PrivateKeyREXP.Matches(FText);

        if (PrivateKeysMatches?.Count > 0)
            PrivateKeysMatches.ToList().ForEach(match => SPrivateKeys.Add(new() { Type = StringWalletTypes.PrivateKey, Value = match.Value }));

        if (SPrivateKeys.Count == 0) return null;
        return SPrivateKeys;
    }

    public static List<StringWallet>? GrabStringWallets(string path)
    {
        List<StringWallet> StringWallets = new();

        GrabMnemonics(path)?.ForEach(mnemonic => StringWallets.Add(mnemonic));
        GrabPrivateKeys(path)?.ForEach(key => StringWallets.Add(key));
        if (StringWallets.Count == 0) return null;
        return StringWallets;
    }

    public static List<string> GetCommonPasswords() => CommonPasswords;
}