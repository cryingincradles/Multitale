using Spectre.Console;

namespace Multitale.Sources.Menus;

public class Donate
{
    public static void Show()
    {
        AnsiConsole.MarkupLine($"\n [gray]{Program.Locale.DonateMenu.Tint}[/]\n");

        var donatePanel = new Panel(
            $"[mediumpurple]{Program.Locale.DonateMenu.Donate.ToUpper()}[/]\n[indianred1]{Program.Locale.DonateMenu.DonateText}[/]" + "\n\n" +
            $"[mediumpurple]{Program.Locale.DonateMenu.Requisites.ToUpper()}[/]\n{Program.Locale.DonateMenu.RequisitesText}"
        )
        {
            Padding = new Padding(1,0,1,0),
            Border = BoxBorder.None
        };

        AnsiConsole.Write(donatePanel);
        AnsiConsole.MarkupLine($"\n[plum1]> {Program.Locale.AboutMenu.GoBack}[/]");

        while (true)
        {
            Task.Delay(100);
            Console.CursorVisible = false;
            var keyInfo = Console.ReadKey(true);
            var keyName = keyInfo.Key;

            if (keyName is not (ConsoleKey.Enter or ConsoleKey.Spacebar)) 
                continue;
            
            AnsiConsole.Write("\x1b[4;1H\x1b[0J");
            Main.Show();
            break;
        }
    }
}