using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Settings
{
    public static void Show()
    {
        // var language = Program.Settings.Main.Language;
        // var proxyPath = Program.Settings.Main.ProxyPath;
        // var viewMode = Program.Settings.Main.ViewMode;
        // var saveDetails = Program.Settings.Main.SaveDetails;

        var selectionPrompt = new SelectionPrompt<string>()
            .Title(
                $"\n [white on mediumpurple] {Program.Locale.SettingsMenu.Settings} [/] [gray]{Program.Locale.SettingsMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle)
                ? parsedStyle ??
                  new Style(Color.Default)
                : new Style(Color.Default))
            .AddChoices(
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Language}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Theme}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ProxyPath}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.ViewMode}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.SaveDetails}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.GoBack}[/]");
        
        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
        
        if (prompt == Program.Locale.SettingsMenu.GoBack)
            Home.Show();
    }

    public static void Language()
    {
        var localeFiles = Directory.GetFiles(Localisation.Localisation.Manager.LocalePath, ".json");
        var localeNames = localeFiles.Select(Path.GetFileNameWithoutExtension).ToArray();
        
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [white on mediumpurple] {Program.Locale.SettingsMenu.Settings} [/] [gray]{Program.Locale.SettingsMenu.Tint}[/]")
            .HighlightStyle(new Style(foreground: Color.Plum1, decoration: Decoration.Dim))
            .AddChoices(
                Program.Locale.SettingsMenu.Language, Program.Locale.SettingsMenu.ProxyPath, 
                Program.Locale.SettingsMenu.ViewMode, Program.Locale.SettingsMenu.SaveDetails,
                Program.Locale.SettingsMenu.GoBack);
    }
}