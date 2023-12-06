using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Home
{
    public static void Show()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.HomeMenu.Home} [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]")
            .HighlightStyle(Style.TryParse($"{Program.Theme.AccentColor} dim", out var parsedStyle) ? 
                parsedStyle ??  new Style(Color.Default) : new Style(Color.Default))
            .AddChoices(
                $"[{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Fetcher}[/]",
                $"[{Program.Theme.DefaultColor}]{Program.Locale.HomeMenu.Settings}[/]", 
                $"[{Program.Theme.DefaultColor}]{Program.Locale.HomeMenu.About}[/]", 
                $"[{Program.Theme.DefaultColor}]{Program.Locale.HomeMenu.Donate}[/]");
        
        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
        
        if (prompt == Program.Locale.HomeMenu.About)
            About.Show();
        else if (prompt == Program.Locale.HomeMenu.Donate)
            Donate.Show();
        else if (prompt == Program.Locale.HomeMenu.Settings)
            Settings.Show();
        else if (prompt == Program.Locale.LauncherMenu.Fetcher)
            Fetcher.Show();
    }
}