using Spectre.Console;

namespace Multitale.Sources.Helpers;

public static class Utils
{
    public static List<string> BufferedReadLines(string filePath)
    {
        var linesList = new List<string>();

        try
        {
            using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var bs = new BufferedStream(fs);
            using var sr = new StreamReader(bs);
            while (sr.ReadLine() is { } line)
            {
                linesList.Add(line);
            }

            return linesList;
        }
        catch (Exception)
        {
            return linesList;
        }
    }
    
    public static void WhilePressed()
    {
        while (true)
        {
            Task.Delay(100);
            Console.CursorVisible = false;
            var keyInfo = Console.ReadKey(true);
            var keyName = keyInfo.Key;

            if (keyName is not (ConsoleKey.Enter or ConsoleKey.Spacebar)) 
                continue;
            break;
        }
    }

    // TODO ClearAndGo<T>() overload for InputMenu
    // public static T? ClearAndGo<T>(Func<T?> element, int clearFrom = 4)
    // {
    //     AnsiConsole.Write($"\x1b[{clearFrom};1H\x1b[0J");
    //     return element();
    // }

    public static void ClearAndGo(Action element, int? clearFrom = null, bool fullCleaning = false)
    {
        if (!fullCleaning)
        {
            clearFrom ??= Program.Logo.Split("\n").Length + 2;
        
            AnsiConsole.Write($"\x1b[{clearFrom};1H\x1b[0J");
        }

        else
        {
            Console.Clear();
            AnsiConsole.Clear();
            Program.ShowLogo();
        }
        
        element();
    }

    // TODO Auto-markup for strings to avoid a lot of trash code
    // public static string CreateMarkup(string text)
    // {
    //     return new string(string.Empty);
    // } 
}