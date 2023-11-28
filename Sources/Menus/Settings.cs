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
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ViewMode}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.SaveDetails}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");
        
        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
        
        if (prompt == Program.Locale.SettingsMenu.GoBack)
        {
            Home.Show();
            return;
        }

        if (prompt == Program.Locale.SettingsMenu.Language)
        {
            Language();
            return;
        }

        if (prompt == Program.Locale.SettingsMenu.Theme)
        {
            Theme();
            return;
        }

        if (prompt == Program.Locale.SettingsMenu.ProxyTimeout)
        {
            ProxyTimeout();
            return;
        }

        Home.Show();
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
            .Where(pair => pair.Alias is not null && pair.LocaleName != Program.Locale.Alias && pair.Alias != Program.Locale.Alias)
            .ToDictionary(pair => pair.Alias!, pair => pair.LocaleName);
        
        var choices = locales
            .Select(el => $"[{Program.Theme.DefaultColor}]{el.Key}[/]")
            .ToList();
        choices.Add($"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");
        
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Language} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
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
            .Where(pair => pair.Alias is not null && pair.ThemeName != Program.Theme.Alias && pair.Alias != Program.Theme.Alias)
            .ToDictionary(pair => pair.Alias!, pair => pair.ThemeName);
        
        var choices = themes
            .Select(el => $"[{Program.Theme.DefaultColor}]{el.Key}[/]")
            .ToList();
        choices.Add($"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");
        
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.Theme} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
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

    public static void ProxyTimeout(bool inputStage = false)
    {
        if (!inputStage)
        {
            var selectionPrompt = new SelectionPrompt<string>()
                .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyTimeout} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.Tint}[/]")
                .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                    parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
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

        AnsiConsole.MarkupLine($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.SettingsMenu.ProxyTimeout} [/] [{Program.Theme.TintColor}]{Program.Locale.SettingsMenu.InputTint}[/]\n");
        Console.CursorVisible = false;
        var textPrompt = new TextPrompt<string>($"[{Program.Theme.DefaultColor}] {Program.Locale.SettingsMenu.InputNumber}:[/]")
            .AllowEmpty();
        var value = AnsiConsole.Prompt(textPrompt);
        var isNumeric = int.TryParse(value, out var valueInt);

        if (value.Length == 0)
        {
            AnsiConsole.Write("\x1b[4;1H\x1b[0J");
            ProxyTimeout();
        }
        
        if (!isNumeric)
        {
            AnsiConsole.MarkupLine($" [{Program.Theme.ErrorColor}]{Program.Locale.SettingsMenu.NotNumber}[/]\n\n[{Program.Theme.AccentColor}]> {Program.Locale.SettingsMenu.GoBackToInput}[/]");
            
            Utils.WhilePressed();
            AnsiConsole.Write("\x1b[4;1H\x1b[0J");
            ProxyTimeout(true);
        }

        else
        {
            Program.Settings.Main.ProxyTimeout = valueInt;
            AnsiConsole.Write("\x1b[4;1H\x1b[0J");
            ProxyTimeout();
        }
    }
}