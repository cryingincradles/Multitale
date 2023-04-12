using System.Text;
using System.Text.RegularExpressions;
using static Structures;

public class Utils
{
    public class IniFile
    {
        private string filePath;

        public IniFile(string filePath)
        {
            this.filePath = filePath;
            if (!File.Exists(filePath)) File.Create(filePath).Close();
        }

        public void Write(string section, string key, string value)
        {
            string line = $"{key}={value}";
            string sectionHeader = $"[{section}]";
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            bool sectionFound = false;
            bool keyFound = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                {
                    if (lines[i].Substring(1, lines[i].Length - 2) == section)
                    {
                        sectionFound = true;
                    }
                    else
                    {
                        sectionFound = false;
                    }
                }
                else if (sectionFound && lines[i].Contains("="))
                {
                    string[] parts = lines[i].Split('=');
                    if (parts[0] == key)
                    {
                        lines[i] = line;
                        keyFound = true;
                        break;
                    }
                }
            }

            if (!keyFound)
            {
                if (!sectionFound)
                {
                    File.AppendAllText(filePath, $"\r\n{sectionHeader}\r\n{line}", Encoding.UTF8);
                }
                else
                {
                    int insertIndex = Array.IndexOf(lines, sectionHeader) + 1;
                    Array.Resize(ref lines, lines.Length + 1);
                    Array.Copy(lines, insertIndex, lines, insertIndex + 1, lines.Length - insertIndex - 1);
                    lines[insertIndex] = line;
                    File.WriteAllLines(filePath, lines, Encoding.UTF8);
                }
            }
            else
            {
                File.WriteAllLines(filePath, lines, Encoding.UTF8);
            }
        }

        public string? Read(string section, string key)
        {
            string? defaultValue = null;
            string[] lines = File.ReadAllLines(filePath, Encoding.UTF8);

            bool sectionFound = false;
            foreach (string line in lines)
            {
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (line.Substring(1, line.Length - 2) == section)
                    {
                        sectionFound = true;
                    }
                    else
                    {
                        sectionFound = false;
                    }
                }
                else if (sectionFound && line.Contains("="))
                {
                    string[] parts = line.Split('=');
                    if (parts[0] == key)
                    {
                        return parts[1];
                    }
                }
            }

            return defaultValue;
        }

        public List<IniData>? ReadSection(string section)
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<IniData>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    if (trimmedLine.Substring(1, trimmedLine.Length - 2) == section)
                    {
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";"))
                {
                    var index = trimmedLine.IndexOf('=');

                    if (index != -1)
                    {
                        var key = trimmedLine.Substring(0, index).Trim();
                        var value = trimmedLine.Substring(index + 1).Trim();

                        result.Add(new()
                        {
                            Section = section,
                            Key = key,
                            Value = value
                        });
                    }
                }
            }

            if (result.Count == 0)
                return null;
            else
                return result;
        }

        public List<IniData>? ReadAll()
        {
            var lines = File.ReadAllLines(filePath);
            var result = new List<IniData>();
            var section = string.Empty;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    section = trimmedLine.Substring(1, trimmedLine.Length - 2);
                }
                else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";") && section != string.Empty)
                {
                    var index = trimmedLine.IndexOf('=');

                    if (index != -1)
                    {
                        var key = trimmedLine.Substring(0, index).Trim();
                        var value = trimmedLine.Substring(index + 1).Trim();

                        result.Add(new()
                        {
                            Section = section,
                            Key = key,
                            Value = value
                        });
                    }
                }
            }

            if (result.Count == 0)
                return null;
            else
                return result;
        }

        public void RewriteAll(List<IniData> data)
        {
            var lines = new List<string>();
            var currentSection = string.Empty;

            foreach (var item in data)
            {
                if (item.Section != currentSection)
                {
                    if (currentSection != string.Empty)
                    {
                        lines.Add("");
                    }

                    lines.Add($"[{item.Section}]");
                    currentSection = item.Section;
                }

                lines.Add($"{item.Key}={item.Value}");
            }

            File.WriteAllLines(filePath, lines);
        }

        public bool IsEmpty()
        {
            return !File.ReadLines(filePath).Any(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(";"));
        }
    }

    private static List<IniData> Defaults = new()
    {
            new() { Section = "Main", Key = "Version", Value = Program.CurrentRelease },
            new() { Section = "Main", Key = "LastVisit", Value = DateTime.Now.ToString("dd.MM.yyyy-H:mm:ss") },
            new() { Section = "Main", Key = "Threads", Value = "40" },
            new() { Section = "Main", Key = "LastPath", Value = "" },
            new() { Section = "Recorder", Key = "WriteTypes", Value = "True" },
            new() { Section = "Output", Key = "SimplifiedView", Value = "False" }
    };

    public static List<IniData> GetDifferentIniData(List<IniData> OldData, List<IniData> NewData)
    {
        var DifferentData = new List<IniData>();

        foreach (IniData OldIniElement in OldData)
        {
            foreach (IniData NewIniElement in NewData)
            {
                if (NewIniElement.Section == OldIniElement.Section && 
                    NewIniElement.Key == OldIniElement.Key && 
                    NewIniElement.Value != OldIniElement.Value)
                {
                    DifferentData.Add(NewIniElement);
                }
            }
        }

        return DifferentData;
    }

    public static void ClearAndShow()
    {
        Console.CursorVisible = false;
        Console.Write("\u001b[2J\u001b[3J");
        Console.Clear();
        Program.ShowLogo();
    }

    public static void LoadDefaults()
    {
        var Settings = new Utils.IniFile("Settings.ini");
        Settings.RewriteAll(Defaults);
    }

    public static List<IniData> GetDefaults()
    {
        return Defaults;
    }

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