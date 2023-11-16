using System.Text;

public class IniFile
{
    private string filePath;
    private object _fileLock = new();

    public struct IniData
    {
        public string Section { get; }
        public string Key { get; }
        public string Value { get; }

        public IniData(string section, string key, string value)
        {
            Section = section;
            Key = key;
            Value = value;
        }
    }

    public IniFile(string filePath)
    {
        this.filePath = filePath;
        if (!File.Exists(filePath)) File.Create(filePath).Close();
    }

    public void Write(string section, string key, string value)
    {
        var line = $"{key}={value}";
        var sectionHeader = $"[{section}]";

        lock (_fileLock)
        {
            var lines = File.ReadAllLines(filePath, Encoding.UTF8);

            var sectionFound = false;
            var keyFound = false;

            for (var i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("[") && lines[i].EndsWith("]"))
                    sectionFound = lines[i][1..^1] == section;

                else if (sectionFound && lines[i].Contains("="))
                {
                    var parts = lines[i].Split('=');
                    if (parts[0] != key) continue;

                    lines[i] = line;
                    keyFound = true;
                    break;
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
    }

    public string? Read(string section, string key)
    {
        string? defaultValue = null;
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        var sectionFound = false;
        foreach (var line in lines)
        {
            if (line.StartsWith("[") && line.EndsWith("]"))
                sectionFound = line[1..^1] == section;

            else if (sectionFound && line.Contains("="))
            {
                var parts = line.Split('=');
                if (parts[0] == key)
                    return parts[1];
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
                if (trimmedLine[1..^1] == section)
                    continue;

                break;
            }

            if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";")) continue;
            var index = trimmedLine.IndexOf('=');

            if (index == -1) continue;
            var key = trimmedLine[..index].Trim();
            var value = trimmedLine[(index + 1)..].Trim();

            result.Add(new(section, key, value));
        }

        return result.Count == 0 ? null : result;
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
                section = trimmedLine[1..^1];
            }
            else if (!string.IsNullOrWhiteSpace(trimmedLine) && !trimmedLine.StartsWith(";") && section != string.Empty)
            {
                var index = trimmedLine.IndexOf('=');

                if (index == -1) continue;
                var key = trimmedLine[..index].Trim();
                var value = trimmedLine[(index + 1)..].Trim();

                result.Add(new(section, key, value));
            }
        }

        return result.Count == 0 ? null : result;
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
                    lines.Add("");

                lines.Add($"[{item.Section}]");
                currentSection = item.Section;
            }

            lines.Add($"{item.Key}={item.Value}");
        }

        File.WriteAllLines(filePath, lines);
    }

    public bool IsEmpty() => !File.ReadLines(filePath)
        .Any(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith(";"));
}

public class Settings
{
    public List<IniFile.IniData> Defaults = new()
    {
        new("Main", "Language", "Russian"),
        new("Main", "ConsoleView", "Default"),
        new("Main", "SaveDetails", "True"),
        new("Main", "ProxiesPath", ""),
        new("Decryptor", "DirectoryPath", ""),
        new("Decryptor", "Fetch", "True"),
        new("Decryptor", "Threads", "40"),
        new("Fetcher", "FilePath", ""),
        new("Fetcher", "Threads", "40")
    };
    
    private static IniFile _settingsIni = new("Settings.ini");

    public Settings()
    {
        if (_settingsIni.IsEmpty()) _settingsIni.RewriteAll(Defaults);
    }
}