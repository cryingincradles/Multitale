﻿using Spectre.Console;

namespace Multitale.Sources.Menus;

public class About
{
    public static void Show()
    {
        AnsiConsole.MarkupLine($"\n [gray]{Program.Locale.AboutMenu.Tint}[/]\n");

        var aboutPanel = new Panel(
            $"[mediumpurple]{Program.Locale.AboutMenu.About.ToUpper()}[/]\n{Program.Locale.AboutMenu.AboutText}" + "\n\n" +
            $"[mediumpurple]{Program.Locale.AboutMenu.Developers.ToUpper()}[/]\n{Program.Locale.AboutMenu.DevelopersText}" + "\n\n" +
            $"[mediumpurple]{Program.Locale.AboutMenu.Links.ToUpper()}[/]\n{Program.Locale.AboutMenu.LinksText}"
            )
        {
            Padding = new Padding(1,0,1,0),
            Border = BoxBorder.None
        };

        AnsiConsole.Write(aboutPanel);
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