using Spectre.Console;

namespace Multitale.Sources.Helpers;

public static class Utils
{
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

    public static void ClearAndGo(Action element, int? clearFrom = null)
    {
        clearFrom ??= Program.Logo.Split("\n").Length + 2;
        
        AnsiConsole.Write($"\x1b[{clearFrom};1H\x1b[0J");
        element();
    }

    // TODO Auto-markup for strings to avoid a lot of trash code
    // public static string CreateMarkup(string text)
    // {
    //     return new string(string.Empty);
    // } 
}