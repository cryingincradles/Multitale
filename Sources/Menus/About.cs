using Multitale.Sources.Helpers;
using Spectre.Console;

namespace Multitale.Sources.Menus;

public class About
{
    public static void Show()
    {
        AnsiConsole.MarkupLine($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.AboutMenu.About} [/] [{Program.Theme.TintColor}]{Program.Locale.AboutMenu.Tint}[/]\n");

        var aboutPanel = new Panel(
            $"[{Program.Theme.DefaultColor}]{Program.Locale.AboutMenu.AboutText}[/]" + "\n\n" +
            $"[{Program.Theme.BaseColor}]{Program.Locale.AboutMenu.Developers}[/]" + "\n" +
            $"[{Program.Theme.DefaultColor}]{Program.Locale.AboutMenu.DevelopersText}[/]" + "\n\n" +
            $"[{Program.Theme.BaseColor}]{Program.Locale.AboutMenu.Links}[/]" + "\n" +
            $"[{Program.Theme.DefaultColor}]{Program.Locale.AboutMenu.LinksText}[/]"
            )
        {
            Padding = new Padding(1,0,1,0),
            Border = BoxBorder.None
        };

        AnsiConsole.Write(aboutPanel);
        AnsiConsole.MarkupLine($"\n[{Program.Theme.AccentColor}]> {Program.Locale.AboutMenu.GoBack}[/]");
        
        Utils.WhilePressed();
        AnsiConsole.Write("\x1b[4;1H\x1b[0J");
        Home.Show();
    }
}