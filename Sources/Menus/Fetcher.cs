using Multitale.Sources.Helpers;
using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Fetcher
{
    public static void Show()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.LauncherMenu.Fetcher} [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
            .AddChoices(
                $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Start}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Settings}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.GoBack}[/]");

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));

        if (prompt == Program.Locale.LauncherMenu.GoBack)
            Home.Show();
        else if (prompt == Program.Locale.LauncherMenu.Settings)
            Settings();
        else if (prompt == Program.Locale.LauncherMenu.Start)
            Start();
    }

    public static void Start()
    {
        AnsiConsole.MarkupLine($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.LauncherMenu.Fetcher} [/] [{Program.Theme.TintColor}]{Program.Locale.LauncherMenu.StartTint}[/]\n");
     
        AnsiConsole.Cursor.Hide();
        Console.CursorVisible = false;
        
        if (Program.Settings.Fetcher.Threads is null)
            AnsiConsole.MarkupLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ThreadsNotSet}");
        
        if (Program.Settings.Fetcher.FilePath is null)
            AnsiConsole.MarkupLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.DataFileNotSet}");
        
        if (Program.Settings.Main.ProxyPath is null)
            AnsiConsole.MarkupLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ProxyFileNotSet}");
        
        if (Program.Settings.Main.ProxyTimeout is null)
            AnsiConsole.WriteLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ProxyTimeoutNotSet}");
        
        if (Program.Settings.Fetcher.Threads is not null && Program.Settings.Fetcher.FilePath is not null)
        {
            var dataFileExists = File.Exists(Program.Settings.Fetcher.FilePath);
            var proxyFileExists = File.Exists(Program.Settings.Main.ProxyPath);
            
            if (!proxyFileExists && Program.Settings.Main.ProxyPath is not null)
                AnsiConsole.MarkupLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ProxyFileNotExists}");
            if (!dataFileExists)
                AnsiConsole.MarkupLine($" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.DataFileNotExists}");
            
            if (proxyFileExists && dataFileExists)
            {
                AnsiConsole.MarkupLine($" [{Program.Theme.BaseColor}]@[/] {Program.Locale.LauncherMenu.ParsingDataFile}");
                var wallets = new List<MultiWallet.Wallet>();
                var proxies = new List<Proxy>();

                wallets = MultiWallet.GetWalletsFromFile(Program.Settings.Fetcher.FilePath);
                AnsiConsole.MarkupLine(wallets.Count == 0
                    ? $" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.DataFileNothingParsed}"
                    : $" [{Program.Theme.BaseColor}]+[/] {Program.Locale.LauncherMenu.CollectedWallets}: {wallets.Count}");

                if (Program.Settings.Main.ProxyPath is not null && proxyFileExists)
                {
                    proxies = Proxy.GetProxyFromFile(Program.Settings.Main.ProxyPath);
                    AnsiConsole.MarkupLine(proxies.Count == 0
                        ? $" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ProxyFileNothingParsed}"
                        : $" [{Program.Theme.BaseColor}]+[/] {Program.Locale.LauncherMenu.CollectedProxy}: {proxies.Count}");
                }

                if (proxies.Count > 0 && wallets.Count > 0)
                {
                    AnsiConsole.MarkupLine($" [{Program.Theme.BaseColor}]@[/] {Program.Locale.LauncherMenu.ValidatingProxy}");
                    var validProxies = new List<Proxy>();
                    Parallel.ForEach(proxies, proxy =>
                    {
                        proxy.Validate();
                        if (proxy.Data.Status.Valid)
                            validProxies.Add(proxy);
                    });
                    
                    AnsiConsole.MarkupLine(validProxies.Count == 0
                        ? $" [{Program.Theme.WarningColor}]![/] {Program.Locale.LauncherMenu.ValidatedProxyNothingParsed}"
                        : $" [{Program.Theme.BaseColor}]+[/] {Program.Locale.LauncherMenu.ValidatedProxy}: {validProxies.Count}");

                    if (validProxies.Count > 0)
                    {
                        AnsiConsole.MarkupLine($" [{Program.Theme.BaseColor}]@[/] {Program.Locale.LauncherMenu.Fetching}\n");
                        var semaphore = new SemaphoreSlim((int)Program.Settings.Fetcher.Threads);
                        var tasks = new List<Task>();
                        var proxyRandom = new Random();
                        
                        foreach (var wallet in wallets)
                        {
                            var walletIndex = wallets.IndexOf(wallet);
                            semaphore.Wait();
                            
                            var task = new Task(() =>
                            {
                                try
                                {
                                    double? cachedErcNetworth = null;
                                    double? cachedTrcNetworth = null;
                                    
                                    while (true)
                                    {
                                        var proxyIndex = proxyRandom.Next(0, validProxies.Count - 1);
                                        var proxy = validProxies[proxyIndex];
                                        var networthTrc = cachedTrcNetworth ?? wallet.Tron.GetNetworth(proxy);;
                                        var networthErc = cachedErcNetworth ?? wallet.Ethereum.GetNetworth(proxy);

                                        if (networthErc is not null)
                                            cachedErcNetworth = networthErc;

                                        if (networthTrc is not null)
                                            cachedTrcNetworth = networthTrc;
                                        
                                        if (networthErc == null || networthTrc == null && (cachedErcNetworth is null || cachedTrcNetworth is null))
                                            continue;
                                        
                                        AnsiConsole.MarkupLine($" [{Program.Theme.BaseColor}]+[/] {Program.Locale.LauncherMenu.Wallet} #{walletIndex + 1}" + "\n" +
                                                               $" [{Program.Theme.BaseColor}]|[/] ERC {Program.Locale.LauncherMenu.Balance}: {cachedErcNetworth}" + "\n" +
                                                               $" [{Program.Theme.BaseColor}]|[/] TRC {Program.Locale.LauncherMenu.Balance}: {cachedTrcNetworth}" + "\n" +
                                                               $" [{Program.Theme.BaseColor}]+[/] {(wallet.Mnemonic is not null ? $"{Program.Locale.LauncherMenu.Mnemonic}: {wallet.Mnemonic.Data}" : $"{Program.Locale.LauncherMenu.PrivateKey}: {wallet.PrivateKey!.Data}")}\n");
                                        break;
                                    }
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            });
                            
                            task.Start();
                            tasks.Add(task);
                        }

                        Task.WhenAll(tasks.ToArray()).GetAwaiter().GetResult();
                    }
                }
            }
        }
        
        AnsiConsole.MarkupLine($"\n[{Program.Theme.AccentColor}]> {Program.Locale.LauncherMenu.GoBack}[/]");
        Utils.WhilePressed();
        Utils.ClearAndGo(Show, fullCleaning:true);
    }
    
    public static void Settings()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Settings} ({Program.Locale.LauncherMenu.Fetcher}) [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
            .AddChoices(
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Threads}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.DataFilePath}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.GoBack}[/]");

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));

        if (prompt == Program.Locale.LauncherMenu.GoBack)
            Show();
        else if (prompt == Program.Locale.SettingsMenu.Threads)
            Threads();
        else if (prompt == Program.Locale.SettingsMenu.DataFilePath)
            DataFilePath();
    }

    public static void Threads(bool inputStage = false)
    {
        if (!inputStage)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Threads} ({Program.Locale.LauncherMenu.Fetcher}) [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]" + "\n"
                    + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Fetcher.Threads}")
                .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                    parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
                .AddChoices(
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Change}[/]",
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.GoBack}[/]");

            var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
            if (prompt == Program.Locale.LauncherMenu.GoBack)
            {
                Settings();
                return;
            }
        }

        AnsiConsole.MarkupLine(
            $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Threads} ({Program.Locale.LauncherMenu.Fetcher}) [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n");

        var textPrompt =
            new TextPrompt<string>($"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputNumber}:[/]")
                .AllowEmpty();
        var value = AnsiConsole.Prompt(textPrompt);
        Console.CursorVisible = false;

        var isNumeric = int.TryParse(value, out var valueInt);

        if (value.Length == 0)
        {
            Utils.ClearAndGo(() => Threads());
            return;
        }

        if (!isNumeric)
        {
            AnsiConsole.MarkupLine(
                $" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.NotNumber}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]");

            Utils.WhilePressed();
            Utils.ClearAndGo(() => Threads(true));
        }

        else
        {
            Program.Settings.Fetcher.Threads = valueInt;
            Utils.ClearAndGo(() => Threads());
        }
    }
    
    public static void DataFilePath(bool inputStage = false)
    {
        if (!inputStage)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                .Title(
                    $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.DataFilePath} ({Program.Locale.LauncherMenu.Fetcher}) [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]" + "\n"
                    + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Fetcher.FilePath ?? Program.Locale.SettingsMenu.NotSet.ToLower()}")
                .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                    ? parsedStyle ?? new Style(Color.Default)
                    : new Style(Color.Default))
                .AddChoices(
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Change}[/]",
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

            var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
            if (prompt == Program.Locale.SettingsMenu.GoBack)
            {
                Settings();
                return;
            }
        }

        AnsiConsole.MarkupLine(
            $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.DataFilePath} ({Program.Locale.LauncherMenu.Fetcher}) [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n");

        var textPrompt =
            new TextPrompt<string>($"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputFilePath}:[/]")
                .AllowEmpty();
        var value = AnsiConsole.Prompt(textPrompt);
        AnsiConsole.Cursor.Hide();

        var isExists = File.Exists(value);

        if (value.Length == 0)
        {
            Utils.ClearAndGo(() => DataFilePath());
            return;
        }

        if (!isExists)
        {
            AnsiConsole.MarkupLine(
                $" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.FileNotExists}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]");

            Utils.WhilePressed();
            Utils.ClearAndGo(() => DataFilePath(true));
        }

        else
        {
            Program.Settings.Fetcher.FilePath = value;
            Utils.ClearAndGo(() => DataFilePath());
        }
    }
}