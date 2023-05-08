using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using Pastel;
using PuppeteerSharp;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using Leaf.xNet;
using static Structures;
using static Structures.Vault;
using System.Diagnostics;

public class Utils
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

    public class ProxyScrapper
    {
        private static bool FromScrapper = false;

        public static Proxy Validate(Proxy Proxy, int MS = 500)
        {
            if (Proxy.Type is null)
            {
                if (Proxy.Port == 80 || Proxy.Port == 8080 || Proxy.Port == 3128)
                    Proxy.Type = Structures.ProxyType.HTTP;

                else if (Proxy.Port == 443 || Proxy.Port == 8443 || Proxy.Port == 563)
                    Proxy.Type = Structures.ProxyType.HTTPS;

                else
                    Proxy.Type = Structures.ProxyType.SOCKS4;
            }

            try
            {
                using var Request = new HttpRequest();
                Request.Proxy = GetProxyClient(Proxy);
                Request.AcceptEncoding = "none";
                Request.Proxy.ConnectTimeout = MS - 50;
                Request.Proxy.ReadWriteTimeout = MS - 50;
                Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36";

                var MSWatcher = new Stopwatch();
                MSWatcher.Start();

                HttpResponse Response = Request.Get("https://api.debank.com/chain/list");

                if (Response.IsOK)
                {
                    if (Request.Proxy is HttpProxyClient && Response.ContainsHeader("Upgrade") && Response["Upgrade"].ToLower() == "tls/1.2") Proxy.Type = Structures.ProxyType.HTTPS;
                    else if (Request.Proxy is HttpProxyClient) Proxy.Type = Structures.ProxyType.HTTP;
                    if ((int)MSWatcher.ElapsedMilliseconds > 1000) return Proxy;

                    MSWatcher.Stop();
                    Proxy.Speed = (int)MSWatcher.ElapsedMilliseconds;
                    return Proxy;
                }

                else
                {
                    Console.WriteLine(Response.StatusCode);
                    return Proxy;
                }
            }

            catch (Exception)
            {
                return Proxy;
            }
        }

        public static List<Proxy> GetProxies(bool ConsoleOutput = true)
        {
            List<Proxy> Proxies = new();
            string FreeProxyList = "https://free-proxy-list.net/";
            string CheckerProxyArchive = "https://checkerproxy.net/api/archive/";
            string RootJazzProxies = "http://rootjazz.com/proxies/proxies.txt";
            string FreeProxyWorld = "https://www.freeproxy.world/?type=&anonymity=&country=&speed=200&port=&page=1";
            string OpenProxySpace = "https://api.openproxy.space/lists/http";

            Console.Title = "Proxy Scrapper";

            if (ConsoleOutput)
                Console.WriteLine($" {"PROXY SCRAPPER LAUNCHED".Pastel(System.Drawing.Color.OrangeRed)}\n\n [{DateTime.Now:HH:mm:ss}] Collecting proxies from {"https://free-proxy-list.net/".Pastel(System.Drawing.Color.LightBlue)}");

            // free-proxy-list.net proxy collector
            using (var Request = new HttpRequest())
            {
                int ProxiesCount = 0;
                try
                {
                    
                    var Response = Request.Get(FreeProxyList);
                    string HtmlContent = Response.ToString();
                    var Doc = new HtmlDocument();
                    Doc.LoadHtml(HtmlContent);
                    var ProxyTable = Doc.DocumentNode.SelectSingleNode("//table[@class='table table-striped table-bordered']/tbody");

                    foreach (var ProxyRow in ProxyTable.SelectNodes(".//tr"))
                    {
                        var Columns = ProxyRow.SelectNodes(".//td");
                        if (Columns != null && Columns.Count > 0)
                        {
                            var Proxy = new Proxy()
                            {
                                IP = Columns[0].InnerText.Trim(),
                                Port = int.Parse(Columns[1].InnerText.Trim())
                            };

                            ProxiesCount++;
                            Proxies.Add(Proxy);
                            Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                        }
                    }
                }
                catch { }

                if (ConsoleOutput)
                {
                    var TryTime = DateTime.Now;
                    Console.WriteLine($" [{TryTime:HH:mm:ss}] Found {ProxiesCount.ToString().Pastel(System.Drawing.Color.Yellow)} proxies");
                }
            }

            if (ConsoleOutput)
                Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] Collecting proxies from {"https://checkerproxy.net/".Pastel(System.Drawing.Color.LightBlue)}");

            // checkerproxy.net proxy collector
            using (var Request = new HttpRequest())
            {
                Request.AcceptEncoding = "none";
                int ProxiesCount = 0;
                int DaysLength = 7;
                var CurrentDate = DateTime.UtcNow;

                try
                {
                    while (true)
                    {
                        if (DaysLength == 0) break;
                        Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                        var Response = Request.Get($"{CheckerProxyArchive}{CurrentDate.AddDays(-DaysLength):yyyy-MM-dd}");
                        Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                        if (Response.IsOK)
                        {
                            var JsonContent = Response.ToString();
                            JArray? Archive = null;

                            try { Archive = JArray.Parse(JsonContent); }
                            catch {   }
                            finally { DaysLength--; }

                            if (Archive is null || Archive.Count == 0) continue;

                            foreach (var Element in Archive)
                            {
                                var IP = Element["addr"]?.ToString().Split(":")[0];
                                var Port = Element["addr"]?.ToString().Split(":")[1];

                                if (IP is null || Port is null) continue;

                                var Proxy = new Proxy()
                                {
                                    IP = IP,
                                    Port = int.Parse(Port)
                                };

                                ProxiesCount++;
                                Proxies.Add(Proxy);
                                Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                            }
                        }
                    }

                    if (ConsoleOutput)
                    {
                        var TryTime = DateTime.Now;
                        Console.WriteLine($" [{TryTime:HH:mm:ss}] Found {(ProxiesCount == 0 ? ProxiesCount.ToString().Pastel(System.Drawing.Color.OrangeRed) : ProxiesCount.ToString().Pastel(System.Drawing.Color.Yellow))} proxies");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            if (ConsoleOutput)
                Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] Collecting proxies from {"http://rootjazz.com/".Pastel(System.Drawing.Color.LightBlue)}");

            // rootjazz.com proxy collector
            using (var Request = new HttpRequest())
            {
                int ProxiesCount = 0;
                try
                {
                    var Response = Request.Get(RootJazzProxies);
                    if (Response.IsOK)
                    {
                        var ProxiesString = Response.ToString();
                        if (ProxiesString is not null)
                        {
                            var ProxiesStrings = ProxiesString.Trim().Split("\n");

                            foreach (var ProxyString in ProxiesStrings)
                            {
                                var Proxy = new Proxy()
                                {
                                    IP = ProxyString.Split(":")[0],
                                    Port = int.Parse(ProxyString.Split(":")[1])
                                };

                                Proxies.Add(Proxy);
                                ProxiesCount++;
                                Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                            }
                        }

                        if (ConsoleOutput)
                        {
                            var TryTime = DateTime.Now;
                            Console.WriteLine($" [{TryTime:HH:mm:ss}] Found {ProxiesCount.ToString().Pastel(System.Drawing.Color.Yellow)} proxies");
                        }
                    }
                }
                catch { }
            }

            if (ConsoleOutput)
                Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] Collecting proxies from {"https://www.freeproxy.world/".Pastel(System.Drawing.Color.LightBlue)}");

            // freeproxy.world proxy collector
            using (var Request = new HttpRequest())
            {
                int ProxiesCount = 0;
                int CurrentPage = 1;
                var CurrentDate = DateTime.UtcNow.Date;
                try
                {
                    var Response = Request.Get(FreeProxyWorld);
                    var FreeProxyWorldPageString = "https://www.freeproxy.world/?type=&anonymity=&country=&speed=200&port=&page=";

                    if (Response.IsOK)
                    {
                        while (true)
                        {
                            if (CurrentPage != 1)
                            {
                                Request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36";
                                Response = Request.Get($"{FreeProxyWorldPageString}{CurrentPage}");
                            }

                            var HtmlContent = Response.ToString();
                            var Doc = new HtmlDocument();
                            Doc.LoadHtml(HtmlContent);
                            var ProxyTableRows = Doc.DocumentNode.SelectNodes("//table[@class='layui-table']/tbody/tr");
                            var NextDisabled = Doc.DocumentNode.SelectSingleNode("//a[@class='layui-laypage-next layui-disabled']");

                            foreach (var ProxyRow in ProxyTableRows)
                            {
                                var Columns = ProxyRow.SelectNodes(".//td");
                                if (Columns != null && Columns.Count > 1)
                                {
                                    var Proxy = new Proxy()
                                    {
                                        IP = Columns[0].InnerText.Trim(),
                                        Port = int.Parse(Columns[1].InnerText.Trim())
                                    };

                                    ProxiesCount++;
                                    Proxies.Add(Proxy);
                                    Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                                }
                            }

                            if (NextDisabled is not null || CurrentPage > 50)
                                break;
                            else
                                CurrentPage++;
                        }

                        if (ConsoleOutput)
                        {
                            var TryTime = DateTime.Now;
                            Console.WriteLine($" [{TryTime:HH:mm:ss}] Found {ProxiesCount.ToString().Pastel(System.Drawing.Color.Yellow)} proxies");
                        }
                    }
                }
                catch { }
            }

            if (ConsoleOutput)
                Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] Collecting proxies from {"https://openproxy.space".Pastel(System.Drawing.Color.LightBlue)}");

            // openproxy.space proxy collector
            using (var Request = new HttpRequest())
            {
                int ProxiesCount = 0;
                try 
                {
                    var Response = Request.Get(OpenProxySpace);

                    if (Response.IsOK)
                    {
                        var ProxyObject = JObject.Parse(Response.ToString());
                        var ProxiesObjects = ProxyObject["data"];
                        if (ProxiesObjects is not null)
                        {
                            foreach (var ProxyCountry in ProxiesObjects)
                            {
                                var ProxyCountryObjects = ProxyCountry["items"];
                                if (ProxyCountryObjects is not null)
                                {
                                    foreach (var ProxiesItem in ProxyCountryObjects)
                                    {
                                        var Proxy = new Proxy()
                                        {
                                            IP = ProxiesItem.ToString().Split(":")[0],
                                            Port = int.Parse(ProxiesItem.ToString().Split(":")[1])
                                        };

                                        Proxies.Add(Proxy);
                                        ProxiesCount++;
                                        Console.Title = $"Proxy Scrapper [{Proxies.Count} proxies]";
                                    }
                                }
                            }

                            if (ConsoleOutput)
                            {
                                var TryTime = DateTime.Now;
                                Console.WriteLine($" [{TryTime:HH:mm:ss}] Found {ProxiesCount.ToString().Pastel(System.Drawing.Color.Yellow)} proxies");
                            }
                        }
                    }

                    else Console.WriteLine(Response.StatusCode);
                }
                catch { }
            }

            return Proxies;
        }

        public static List<Proxy> CallScrapper(string ResultsPath)
        {
            ClearAndShow();
            FromScrapper = true;

            var Proxies = GetProxies();

            if (Proxies.Count > 0)
                Proxies = CallValidator(ResultsPath, Proxies);

            return Proxies;
        }

        public static List<Proxy> CallValidator(string ResultsPath, List<Proxy> Proxies)
        {
            if (FromScrapper is false) ClearAndShow();

            var ValidProxies = new List<Proxy>();
            var Settings = new IniFile("Settings.ini");
            bool WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            var Partitions = Partitioner.Create(Proxies).GetPartitions(40);
            var Tasks = new List<Task>();
            int ValidatedCounter = 0;
            Console.Title = $"Validating [0/{Proxies.Count} proxies]";
            Console.WriteLine($"{(FromScrapper is true ? "\n" : "")} {$"VALIDATOR LAUNCHED".ToString().Pastel(System.Drawing.Color.OrangeRed)}\n");

            foreach (var Partition in Partitions)
            {
                var task = Task.Run(() =>
                {
                    while (Partition.MoveNext())
                    {
                        try
                        {
                            var VProxy = Validate(Partition.Current);

                            if (VProxy.Speed is null && WriteTypes is true)
                            {
                                TryCreateDirectory(ResultsPath);
                                Recorders.Universal.Record(ResultsPath, VProxy);
                            }

                            if (VProxy.Speed is not null)
                            {
                                TryCreateDirectory(ResultsPath);
                                Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] {VProxy.Type} => {$"{VProxy.IP}:{VProxy.Port}".Pastel(System.Drawing.Color.Yellow)} => {VProxy.Speed}ms");
                                Recorders.Universal.Record(ResultsPath, VProxy);
                                ValidProxies.Add(VProxy);
                            }

                            ValidatedCounter++;
                            Console.Title = $"Validating [{ValidatedCounter}/{Proxies.Count} proxies]";
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                });

                Tasks.Add(task);
            }

            Task.WaitAll(Tasks.ToArray());
            FromScrapper = false;
            Console.Title = $"";

            return Proxies;
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
        Console.Write("\u001b[2J\u001b[3J");
        Console.Clear();
        Program.ShowLogo();
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

    public static List<Proxy>? GrabProxies (string path)
    {
        List<Proxy> Proxies = new();
        var FileData = File.ReadAllText(path);
        var ProxiesRegex = new System.Text.RegularExpressions.Regex("^(\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}):(\\d{1,5})(\\:(\\w*):(\\w*))?$", RegexOptions.Multiline);
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
                    Password = SplittedData.Length > 3 ? SplittedData[3]: null
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

        var MnemonicExp = new string("^([a-z]+ ){11}[a-z]+$|^([a-z]+ ){14}[a-z]+$|^([a-z]+ ){17}[a-z]+$|^([a-z]+ ){20}[a-z]+$|^([a-z]+ ){23}[a-z]+$");
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