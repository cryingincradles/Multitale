using Multitale.Sources.Helpers;
using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Donate
{
    public static void Show()
    {
        AnsiConsole.MarkupLine($"\n [{Program.Theme.DefaultColor} on {Program.Theme.BaseColor}] {Program.Locale.DonateMenu.Donate} [/] [{Program.Theme.TintColor}]{Program.Locale.DonateMenu.Tint}[/]\n");

        var donatePanel = new Panel(
            $"[{Program.Theme.WarningColor}]{Program.Locale.DonateMenu.DonateText}[/]" + "\n\n" +
            $"[{Program.Theme.BaseColor}]{Program.Locale.DonateMenu.Requisites}[/]" + "\n" +
            $"[{Program.Theme.DefaultColor}]{Program.Locale.DonateMenu.RequisitesText}[/]"
        )
        {
            Padding = new Padding(1,0,1,0),
            Border = BoxBorder.None
        };

        AnsiConsole.Write(donatePanel);
        AnsiConsole.MarkupLine($"\n[{Program.Theme.AccentColor}]> {Program.Locale.AboutMenu.GoBack}[/]");
       
        Utils.WhilePressed();
        AnsiConsole.Write("\x1b[4;1H\x1b[0J");
        Home.Show();
    }
}