using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Fetcher
{
    public static void Show()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.LauncherMenu.Fetcher} [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]")
            .AddChoices(
                $" [{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Start}[/]",
                $" [{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Settings}[/]",
                $" [{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.GoBack}[/]");

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));

        if (prompt == Program.Locale.LauncherMenu.GoBack)
        {
            Home.Show();
            return;
        }

        // TODO Fetcher settings
        // if (prompt == Program.Locale.SettingsMenu)
        // {
        //     
        // }
    }

    public static void Settings()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.LauncherMenu.Fetcher} [/] [{Program.Theme.TintColor}]{Program.Locale.HomeMenu.Tint}[/]")
            .AddChoices(
                $" [{Program.Theme.DefaultColor}]{Program.Locale.SettingsMenu.Threads}[/]",
                $" [{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.Settings}[/]",
                $" [{Program.Theme.DefaultColor}]{Program.Locale.LauncherMenu.GoBack}[/]");

        var prompt = Markup.Remove(AnsiConsole.Prompt(selectionPrompt));
    }
}