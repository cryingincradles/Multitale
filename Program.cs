#pragma warning disable CS0649

using Pastel;

class Program 
{
    public static string CurrentRelease = "v0.2.0-alpha";

    public static void ShowLogo()
    {
        Console.WriteLine(("" +
    "  __  __      _ _   _ _        _     \r\n " +
    "|  \\/  |_  _| | |_|_| |_ __ _| |___ \r\n " +
    "| |\\/| | || | |  _| |  _/ _` | / -_)\r\n " +
    "|_|  |_|\\_,_|_|\\__|_|\\__\\__,_|_\\___|\r\n").Pastel(System.Drawing.Color.OrangeRed) +
    $" {CurrentRelease}\r\n".Pastel(System.Drawing.Color.Orange));
    }

    public static void Main()
    {
        Console.CursorVisible = false;
        Utils.IniFile Settings = new Utils.IniFile("Settings.ini");
        if (Settings.IsEmpty()) Utils.LoadDefaults();

        Settings.Write("Main", "LastVisit", DateTime.Now.ToString("dd.MM.yyyy-H:mm:ss"));
        Settings.Write("Main", "Version", CurrentRelease);

        Menu.Main.Show();
    }
}