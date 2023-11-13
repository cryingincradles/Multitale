using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using Leaf.xNet;
using static Structures;
using System.Diagnostics;
using Spectre.Console;

public partial class Utils
{
    public class ProxyScrapper
    {
        private static bool FromScrapper = false;

        public static object? Validate(Proxy Proxy, int MS = 5000, string? ResourceUrl = null)
        {
            Proxy.Type ??= GetProxyType(Proxy.Port);
            string RequestUrl = ResourceUrl is null ? "https://dns.google/" : ResourceUrl;

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
                var Response = Request.Get(RequestUrl);
                
                if (Response.IsOK)
                {
                    if (Request.Proxy is HttpProxyClient && Response.ContainsHeader("Upgrade") && Response["Upgrade"].ToLower() == "tls/1.2") Proxy.Type = Structures.ProxyType.HTTPS;
                    else if (Request.Proxy is HttpProxyClient) Proxy.Type = Structures.ProxyType.HTTP;
                    if ((int)MSWatcher.ElapsedMilliseconds > MS) return Proxy;

                    MSWatcher.Stop();
                    Proxy.Speed = (int)MSWatcher.ElapsedMilliseconds;
                    var CachedResponse = Response.ToString();

                    if (ResourceUrl is null) return Proxy;
                    else return Response;
                }

                else
                {
                    throw new Exception("Response is not OK");
                }
            }

            catch (Exception)
            {
                if (Proxy.Login is not null && Proxy.Password is not null && Proxy.Type is not Structures.ProxyType.SOCKS5)
                {
                    Proxy.Type = Structures.ProxyType.SOCKS5;
                }

                else if (Proxy.Type is Structures.ProxyType.HTTP)
                {
                    Proxy.Type = Structures.ProxyType.SOCKS4;
                }

                else if (Proxy.Type is Structures.ProxyType.SOCKS4)
                {
                    Proxy.Type = Structures.ProxyType.SOCKS5;
                }

                else if (Proxy.Type is Structures.ProxyType.SOCKS5)
                {
                    if (ResourceUrl is not null) return null;
                    else return Proxy;
                }

                return Validate(Proxy, ResourceUrl: ResourceUrl);
            }
        }

        public static List<Proxy> GetProxies()
        {
            List<Proxy> Proxies = new();
            string FreeProxyList = "https://free-proxy-list.net/";
            string CheckerProxyArchive = "https://checkerproxy.net/api/archive/";
            string RootJazzProxies = "http://rootjazz.com/proxies/proxies.txt";
            string FreeProxyWorld = "https://www.freeproxy.world/?type=&anonymity=&country=&speed=200&port=&page=1";
            string OpenProxySpace = "https://api.openproxy.space/lists/http";
            string DefaultTitle = "~ Scrapper";
            Console.Title = DefaultTitle;

            var LinesSpinner = new CustomSpinners.Lines();

            try
            {
                AnsiConsole.MarkupLine(" [mediumpurple]#[/] Scrapping started");
                AnsiConsole.Status()
                    .Spinner(LinesSpinner)
                    .SpinnerStyle(Style.Parse("mediumpurple"))
                    .Start(" [mediumpurple]@[/] Collecting proxies from public resources...", ctx =>
                    {
                        Thread.Sleep(2000);

                        // free-proxy-list.net proxy collector
                        using (var Request = new HttpRequest())
                        {
                            ctx.Status = " [mediumpurple]@[/] https://free-proxy-list.net/";

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
                                        Console.Title = $"{DefaultTitle} {{{Proxies.Count} proxies}}";
                                    }
                                }
                            }
                            catch { }
                            finally
                            {
                                AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{ProxiesCount}}}[/] proxies from [mediumpurple]{{https://free-proxy-list.net}}[/]");
                            }
                        }

                        // checkerproxy.net proxy collector
                        using (var Request = new HttpRequest())
                        {
                            Request.AcceptEncoding = "none";
                            int ProxiesCount = 0;
                            int DaysLength = 7;
                            var CurrentDate = DateTime.UtcNow;
                            ctx.Status = " [mediumpurple]@[/] https://checkerproxy.net/";

                            while (true)
                            {
                                if (DaysLength == 0) break;
                                HttpResponse? Response = null;

                                try { Response = Request.Get($"{CheckerProxyArchive}{CurrentDate.AddDays(-DaysLength):yyyy-MM-dd}"); }
                                catch { }
                                finally { DaysLength--; }

                                if (Response is null) continue;

                                if (Response.IsOK)
                                {
                                    var JsonContent = Response.ToString();
                                    JArray? Archive = null;

                                    try { Archive = JArray.Parse(JsonContent); }
                                    catch { }

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
                                        Console.Title = $"{DefaultTitle} {{{Proxies.Count} proxies}}";
                                    }
                                }
                            }

                            AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{ProxiesCount}}}[/] proxies from [mediumpurple]{{https://checkerproxy.net}}[/]");
                        }

                        // rootjazz.com proxy collector
                        using (var Request = new HttpRequest())
                        {
                            int ProxiesCount = 0;
                            ctx.Status = " [mediumpurple]@[/] https://rootjazz.com/";

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
                                            Console.Title = $"{DefaultTitle} {{{Proxies.Count} proxies}}";
                                        }
                                    }
                                }
                            }
                            catch { }
                            finally
                            {
                                AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{ProxiesCount}}}[/] proxies from [mediumpurple]{{https://rootjazz.com}}[/]");
                            }
                        }

                        // freeproxy.world proxy collector
                        using (var Request = new HttpRequest())
                        {
                            int ProxiesCount = 0;
                            int CurrentPage = 1;
                            var CurrentDate = DateTime.UtcNow.Date;
                            ctx.Status = " [mediumpurple]@[/] https://freeproxy.world/";

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
                                                Console.Title = $"{DefaultTitle} {{{Proxies.Count} proxies}}";
                                            }
                                        }

                                        if (NextDisabled is not null || CurrentPage > 50)
                                            break;
                                        else
                                            CurrentPage++;
                                    }
                                }
                            }
                            catch { }
                            finally
                            {
                                AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{ProxiesCount}}}[/] proxies from [mediumpurple]{{https://freeproxy.world}}[/]");
                            }
                        }

                        // openproxy.space proxy collector
                        using (var Request = new HttpRequest())
                        {
                            int ProxiesCount = 0;
                            ctx.Status = " [mediumpurple]@[/] https://openproxy.space/";

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
                                                    Console.Title = $"{DefaultTitle} {{{Proxies.Count} proxies}}";
                                                }
                                            }
                                        }
                                    }
                                }

                                else Console.WriteLine(Response.StatusCode);
                            }
                            catch { }
                            finally
                            {
                                AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{ProxiesCount}}}[/] proxies from [mediumpurple]{{https://openproxy.space}}[/]");
                            }
                        }

                        AnsiConsole.MarkupLine(" [mediumpurple]#[/] Scrapping is done");
                    });

                if (Proxies.Count > 0)
                {
                    AnsiConsole.WriteLine();

                    Proxies = Proxies.Select(el => { el.Type = GetProxyType(el.Port); return el; }).ToList();
                    var BChart = new BarChart();
                    BChart.Width = 60;

                    var HTTPProxies = Proxies.Where(el => el.Type == Structures.ProxyType.HTTP).ToList()?.Count;
                    var HTTPSProxies = Proxies.Where(el => el.Type == Structures.ProxyType.HTTPS).ToList()?.Count;
                    var SOCKS4Proxies = Proxies.Where(el => el.Type == Structures.ProxyType.SOCKS4).ToList()?.Count;
                    var SOCKS5Proxies = Proxies.Where(el => el.Type == Structures.ProxyType.SOCKS5).ToList()?.Count;

                    var ProxiesCounts = new Dictionary<string, int>();
                    if (HTTPProxies is not null) ProxiesCounts.Add("HTTP", HTTPProxies.Value);
                    if (HTTPSProxies is not null) ProxiesCounts.Add("HTTPs", HTTPSProxies.Value);
                    if (SOCKS4Proxies is not null) ProxiesCounts.Add("SOCKS 4", SOCKS4Proxies.Value);
                    if (SOCKS5Proxies is not null) ProxiesCounts.Add("SOCKS 5", SOCKS5Proxies.Value);

                    var OrderedProxiesCounts = ProxiesCounts.OrderByDescending(el => el.Value);
                    var PColors = new Dictionary<int, Color>()
                    {
                        { 1, Color.Plum1 },
                        { 2, Color.Plum2 },
                        { 3, Color.Plum3 },
                        { 4, Color.Plum4 }
                    };

                    var CurrentProxieIndex = 1;
                    foreach (var OPE in OrderedProxiesCounts)
                    {
                        BChart.AddItem($" {OPE.Key}", OPE.Value, PColors[CurrentProxieIndex]);
                        CurrentProxieIndex++;
                    }

                    AnsiConsole.Write(BChart);
                }
                else AnsiConsole.MarkupLine("\n [black on plum2] No any proxies was scrapped [/]\n");
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                Thread.Sleep(3000);
            }

            return Proxies;
        }

        public static Structures.ProxyType GetProxyType(int Port) 
        {
            Structures.ProxyType PType;

            if (Port == 80 || Port == 8080 || Port == 3128)
                PType = Structures.ProxyType.HTTP;

            else if (Port == 443 || Port == 8443 || Port == 563)
                PType = Structures.ProxyType.HTTPS;

            else
                PType = Structures.ProxyType.HTTP;

            return PType;
        }

        public static List<Proxy> CallScrapper(string ResultsPath)
        {
            FromScrapper = true;

            var Proxies = GetProxies();

            if (Proxies.Count > 0)
                Proxies = CallValidator(ResultsPath, Proxies);

            return Proxies;
        }

        public static List<Proxy> CallValidator(string ResultsPath, List<Proxy> Proxies)
        {
            if (FromScrapper is true) AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine(" [mediumpurple]#[/] Validating started");
            var ValidProxies = new List<Proxy>();
            var Settings = new IniFile("Settings.ini");
            bool WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            var Partitions = Partitioner.Create(Proxies).GetPartitions(40);
            var Tasks = new List<Task>();
            int ValidatedCounter = 0;
            Console.Title = $"~ Validator {{0/{Proxies.Count} proxies}}";
            var LinesSpinner = new CustomSpinners.Lines();

            AnsiConsole.Status()
                .Spinner(LinesSpinner)
                .SpinnerStyle(Style.Parse("mediumpurple"))
                .Start(" [mediumpurple]@[/] Validating...", ctx =>
                {
                    Thread.Sleep(3000);

                    int PartionIndex = 1;
                    foreach (var Partition in Partitions)
                    {
                        var task = Task.Run(() =>
                        {
                            while (Partition.MoveNext())
                            {
                                try
                                {
                                    var VProxy = (Proxy?)Validate(Partition.Current);

                                    if (VProxy is not null)
                                    {
                                        if (VProxy.Value.Speed is null && WriteTypes is true)
                                        {
                                            TryCreateDirectory(ResultsPath);
                                            Recorders.Universal.Record(ResultsPath, VProxy);
                                        }

                                        if (VProxy.Value.Speed is not null)
                                        {
                                            TryCreateDirectory(ResultsPath);
                                            AnsiConsole.MarkupLine($" [mediumpurple]>[/] Proxy [mediumpurple]{{{VProxy.Value.IP}:{VProxy.Value.Port}}}[/] was connected in [mediumpurple]{{{VProxy.Value.Speed}ms}}[/]");
                                            Recorders.Universal.Record(ResultsPath, VProxy);
                                            ValidProxies.Add(VProxy.Value);
                                        }
                                    }

                                    ValidatedCounter++;
                                    Console.Title = $"~ Validator {{{ValidatedCounter}/{Proxies.Count} proxies}}";
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        });

                        Tasks.Add(task);
                        PartionIndex++;
                    }

                    Task.WaitAll(Tasks.ToArray());
                });

            AnsiConsole.MarkupLine(" [mediumpurple]#[/] Validating is done");

            if (ValidProxies.Count > 0)
            {
                AnsiConsole.WriteLine();

                var BChart = new BarChart();
                BChart.Width = 60;

                var HTTPProxies = ValidProxies.Where(el => el.Type == Structures.ProxyType.HTTP).ToList()?.Count;
                var HTTPSProxies = ValidProxies.Where(el => el.Type == Structures.ProxyType.HTTPS).ToList()?.Count;
                var SOCKS4Proxies = ValidProxies.Where(el => el.Type == Structures.ProxyType.SOCKS4).ToList()?.Count;
                var SOCKS5Proxies = ValidProxies.Where(el => el.Type == Structures.ProxyType.SOCKS5).ToList()?.Count;

                var ProxiesCounts = new Dictionary<string, int>();
                if (HTTPProxies is not null) ProxiesCounts.Add("HTTP", HTTPProxies.Value);
                if (HTTPSProxies is not null) ProxiesCounts.Add("HTTPs", HTTPSProxies.Value);
                if (SOCKS4Proxies is not null) ProxiesCounts.Add("SOCKS 4", SOCKS4Proxies.Value);
                if (SOCKS5Proxies is not null) ProxiesCounts.Add("SOCKS 5", SOCKS5Proxies.Value);

                var OrderedProxiesCounts = ProxiesCounts.OrderByDescending(el => el.Value);
                var PColors = new Dictionary<int, Color>()
                {
                    { 1, Color.Plum1 },
                    { 2, Color.Plum2 },
                    { 3, Color.Plum3 },
                    { 4, Color.Plum4 }
                 };

                var CurrentProxieIndex = 1;
                foreach (var OPE in OrderedProxiesCounts)
                {
                    BChart.AddItem($" {OPE.Key}", OPE.Value, PColors[CurrentProxieIndex]);
                    CurrentProxieIndex++;
                }

                AnsiConsole.Write(BChart);
            }

            else AnsiConsole.MarkupLine("\n [black on mediumpurple] No any valid proxies [/]\n");

            FromScrapper = false;
            Console.Title = $"~ Multitale";

            return Proxies;
        }
    }
}