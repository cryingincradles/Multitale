using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Main
{
    public static void Show()
    {
        var selectionPrompt = new SelectionPrompt<string>()
            .Title($"\n [gray]{Program.Locale.MainMenu.Tint}[/]")
            .HighlightStyle(new Style(foreground: Color.Plum1, decoration: Decoration.Dim))
            .AddChoices(
                Program.Locale.LauncherMenu.Fetcher, Program.Locale.MainMenu.Settings, 
                Program.Locale.MainMenu.About, Program.Locale.MainMenu.Donate);
        
        var prompt = AnsiConsole.Prompt(selectionPrompt);
        if (prompt == Program.Locale.MainMenu.About)
            About.Show();
    }
}