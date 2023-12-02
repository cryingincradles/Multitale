using Multitale.Sources.Helpers;
using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Settings
{
    public static void Show()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title(
                $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Settings} [/] [gray]{Program.Locale.SettingsMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                ? parsedStyle ??
                  new Style(Color.Default)
                : new Style(Color.Default))
            .AddChoices(
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Language}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Theme}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ProxyPath}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ProxyTimeout}[/]",
                // TODO Create output modes system
                //$"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ViewMode}[/]",
                // TODO Create different saving method
                // $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.SaveDetails}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));

        if (prompt == Program.Locale.SettingsMenu.GoBack)
            Home.Show();
        else if (prompt == Program.Locale.SettingsMenu.Language)
            Language();
        else if (prompt == Program.Locale.SettingsMenu.Theme)
            Theme();
        else if (prompt == Program.Locale.SettingsMenu.ProxyTimeout)
            ProxyTimeout();
        else if (prompt == Program.Locale.SettingsMenu.ProxyPath)
            ProxyPath();
        else Home.Show();
    }

    public static void Language()
    {
        var localesFiles = Directory.GetFiles(Localisation.Localisation.Manager.LocalesPath, "*.json");
        var locales = localesFiles
            .Select(el =>
            {
                var localeName = Path.GetFileNameWithoutExtension(el);
                var locale = Localisation.Localisation.Manager.GetLocaleFromFile(el);
                return new { LocaleName = localeName, locale?.Alias };
            })
            .Where(pair => pair.Alias is not null && pair.LocaleName != Program.Locale.Alias &&
                           pair.Alias != Program.Locale.Alias)
            .ToDictionary(pair => pair.Alias!, pair => pair.LocaleName);

        var choices = locales
            .Select(el => $"[{Program.Theme.DefaultColor}]{el.Key}[/]")
            .ToList();
        choices.Add($"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

        var selectionPrompt = new SelectionPrompt<string>()
            .Title(
                $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Language} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]" + "\n"
                + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Main.Language}")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                ? parsedStyle ?? new Style(Color.Default)
                : new Style(Color.Default))
            .AddChoices(choices: choices);

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
        if (prompt == Program.Locale.SettingsMenu.GoBack)
        {
            Show();
            return;
        }

        Program.Settings.Main.Language = locales[prompt];
        Language();
    }

    public static void Theme()
    {
        var themesFiles = Directory.GetFiles(Themes.Themes.Manager.ThemesPath, "*.json");
        var themes = themesFiles
            .Select(el =>
            {
                var themeName = Path.GetFileNameWithoutExtension(el);
                var theme = Themes.Themes.Manager.GetThemeFromFile(el);
                return new { ThemeName = themeName, theme?.Alias };
            })
            .Where(pair => pair.Alias is not null && pair.ThemeName != Program.Theme.Alias &&
                           pair.Alias != Program.Theme.Alias)
            .ToDictionary(pair => pair.Alias!, pair => pair.ThemeName);

        var choices = themes
            .Select(el => $"[{Program.Theme.DefaultColor}]{el.Key}[/]")
            .ToList();
        choices.Add($"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

        var selectionPrompt = new SelectionPrompt<string>()
            .Title(
                $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Theme} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]" + "\n"
                + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Main.Theme}")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                ? parsedStyle ?? new Style(Color.Default)
                : new Style(Color.Default))
            .AddChoices(choices: choices);

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
        if (prompt == Program.Locale.SettingsMenu.GoBack)
        {
            Show();
            return;
        }

        Program.Settings.Main.Theme = themes[prompt];
        Program.RebuildConsole();
        Theme();
    }

    public static void ProxyPath(bool inputStage = false)
    {
        if (!inputStage)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                .Title(
                    $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyPath} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]" + "\n"
                    + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Main.ProxyPath ?? Program.Locale.SettingsMenu.NotSet.ToLower()}")
                .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                    ? parsedStyle ?? new Style(Color.Default)
                    : new Style(Color.Default))
                .AddChoices(
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Change}[/]",
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

            var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
            if (prompt == Program.Locale.SettingsMenu.GoBack)
            {
                Show();
                return;
            }
        }

        AnsiConsole.MarkupLine(
            $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyPath} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n");

        var textPrompt =
            new TextPrompt<string>($"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputFilePath}:[/]")
                .AllowEmpty();
        var value = AnsiConsole.Prompt(textPrompt);
        Console.CursorVisible = false;

        var isExists = File.Exists(value);

        if (value.Length == 0)
        {
            Utils.ClearAndGo(() => ProxyPath());
            return;
        }

        if (!isExists)
        {
            AnsiConsole.MarkupLine(
                $" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.FileNotExists}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]");

            Utils.WhilePressed();
            Utils.ClearAndGo(() => ProxyPath(true));
        }

        else
        {
            Program.Settings.Main.ProxyPath = value;
            Utils.ClearAndGo(() => ProxyPath());
        }
    }

    public static void ProxyTimeout(bool inputStage = false)
    {
        if (!inputStage)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                .Title(
                    $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyTimeout} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]" + "\n"
                    + $" ~ [{Program.Theme.BaseColor}]{Program.Locale.SettingsMenu.CurrentValue}:[/] {Program.Settings.Main.ProxyTimeout}")
                .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                    ? parsedStyle ?? new Style(Color.Default)
                    : new Style(Color.Default))
                .AddChoices(
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Change}[/]",
                    $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");

            var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
            if (prompt == Program.Locale.SettingsMenu.GoBack)
            {
                Show();
                return;
            }
        }

        // TODO Call input menu from InputMenu<T> class
        // var pst = Program.Settings.Main.ProxyTimeout.GetType();
        //
        // var inputMenu =
        //     new Menu.InputMenu<int>(
        //         $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyTimeout} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n",
        //         $"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputNumber}:[/]",
        //         $" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.NotNumber}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]",
        //         () => ProxyTimeout(),
        //         ProxyT);
        //
        // var inputValue = inputMenu.Show();
        // Program.Settings.Main.ProxyTimeout = inputValue;
        // Utils.ClearAndGo(() => ProxyTimeout());

        AnsiConsole.MarkupLine(
            $"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyTimeout} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n");

        var textPrompt =
            new TextPrompt<string>($"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputNumber}:[/]")
                .AllowEmpty();
        var value = AnsiConsole.Prompt(textPrompt);
        Console.CursorVisible = false;

        var isNumeric = int.TryParse(value, out var valueInt);

        if (value.Length == 0)
        {
            Utils.ClearAndGo(() => ProxyTimeout());
            return;
        }

        if (!isNumeric)
        {
            AnsiConsole.MarkupLine(
                $" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.NotNumber}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]");

            Utils.WhilePressed();
            Utils.ClearAndGo(() => ProxyTimeout(true));
        }

        else
        {
            Program.Settings.Main.ProxyTimeout = valueInt;
            Utils.ClearAndGo(() => ProxyTimeout());
        }
    }
    
    // TODO View mode for AnsiConsole output
    // public static void ViewMode()
    // {
    // }
}