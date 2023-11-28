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
}