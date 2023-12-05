namespace Multitale.Sources.Helpers;

public class Menu
{
    // TODO Place same menus in one class base
    // public class InputMenu<T, TU>
    // {
    //     private readonly string _headText;
    //     private readonly string _promptText;
    //     private readonly string _badTypeErrorText;
    //     private readonly Action _parentMenuAction;
    //     private readonly Settings _settings = Program.Settings;
    //     private readonly TU _property;
    //     
    //     public InputMenu(string headText, string promptText, string badTypeErrorText, Action parentMenuAction, TU property)
    //     {
    //         _headText = headText;
    //         _badTypeErrorText = badTypeErrorText;
    //         _promptText = promptText;
    //         _parentMenuAction = parentMenuAction;
    //         _property = property;
    //     }
    //
    //     public T Show()
    //     {
    //         AnsiConsole.MarkupLine(_headText);
    //         var textPrompt = new TextPrompt<string>(_promptText)
    //             .AllowEmpty();
    //         var stringValue = AnsiConsole.Prompt(textPrompt);
    //         Console.CursorVisible = false;
    //
    //         if (stringValue.Length == 0)
    //             Utils.ClearAndGo(_parentMenuAction);
    //
    //         if (TryParse(stringValue, out var parsedValue))
    //             return parsedValue!;
    //         
    //
    //         AnsiConsole.MarkupLine(_badTypeErrorText);
    //         Utils.WhilePressed();
    //         return Utils.ClearAndGo(Show)!;
    //     }
    //
    //     private bool TryParse(string inValue, out T? outValue)
    //     {
    //         var converter = TypeDescriptor.GetConverter(typeof(T));
    //
    //         try
    //         {
    //             outValue = (T)converter.ConvertFromString(null,
    //                 CultureInfo.InvariantCulture, inValue)!;
    //             return true;
    //         }
    //
    //         catch
    //         {
    //             // ignored
    //         }
    //
    //         outValue = default;
    //         return false;
    //     }
    // }
}