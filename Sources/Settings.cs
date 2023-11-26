using Multitale.Sources.Helpers;

namespace Multitale.Sources;

public class Settings
{
    private static IniFile _settingsIni = new("Settings.ini");
    
    public IFetcher Fetcher;
    public IMain Main;
    public IDecoder Decryptor;
    
    public List<IniFile.IniData> Defaults = new()
    {
        new("Main", "Language", "English"),
        new("Main", "Theme", "Purple"),
        new("Main", "ViewMode", "Default"),
        new("Main", "SaveDetails", "True"),
        new("Main", "ProxyPath", ""),
        new("Main", "ProxyTimeout", "2000"),
        new("Decoder", "DirectoryPath", ""),
        new("Decoder", "Threads", "40"),
        new("Decoder", "Fetch", "True"),
        new("Fetcher", "FilePath", ""),
        new("Fetcher", "Threads", "40")
    };

    public Settings()
    {
        if (_settingsIni.IsEmpty())
        {
            Program.Log.Warning("Settings file is empty. Loading defaults...");
            _settingsIni.RewriteAll(Defaults);
            Program.Log.Warning("Settings defaults loaded");
        };
        Fetcher = new IFetcher();
        Main = new IMain();
        Decryptor = new IDecoder();
    }

    public class IMain
    {
        public string? Language
        {
            get => _settingsIni.Read("Main", "Language");
            set => _settingsIni.Write("Main", "Language", value ?? "English");
        }
        
        public string? Theme
        {
            get => _settingsIni.Read("Main", "Theme");
            set => _settingsIni.Write("Main", "Theme", value ?? "Purple");
        }
        
        public string? ViewMode
        {
            get => _settingsIni.Read("Main", "ViewMode");
            set => _settingsIni.Write("Main", "ViewMode", value ?? "");
        }
        
        public bool? SaveDetails
        {
            get => bool.TryParse(_settingsIni.Read("Main", "SaveDetails"), out var result) ? result : null;
            set => _settingsIni.Write("Main", "SaveDetails", value is null ? "True" : $"{value}");
        }
        
        public string? ProxyPath
        {
            get => _settingsIni.Read("Main", "ProxyPath");
            set => _settingsIni.Write("Main", "ProxyPath", value ?? "");
        }
        
        public int? ProxyTimeout
        {
            get => int.TryParse(_settingsIni.Read("Main", "ProxyTimeout"), out var result) ? result : null;
            set => _settingsIni.Write("Main", "ProxyTimeout", value is null ? "2000" : $"{value}");
        }
    }

    public class IDecoder
    {
        public string? DirectoryPath
        {
            get => _settingsIni.Read("Decoder", "DirectoryPath");
            set => _settingsIni.Write("Decoder", "DirectoryPath", value ?? "");
        }
        
        public int? Threads
        {
            get => int.TryParse(_settingsIni.Read("Decoder", "Threads"), out var result) ? result : null;
            set => _settingsIni.Write("Decoder", "Threads", value is null ? "True" : $"{value}");
        }
        
        public bool? Fetch
        {
            get => bool.TryParse(_settingsIni.Read("Decoder", "Fetch"), out var result) ? result : null;
            set => _settingsIni.Write("Decoder", "Fetch", value is null ? "True" : $"{value}");
        }
    }
    
    public class IFetcher
    {   
        public string? FilePath
        {
            get => _settingsIni.Read("Fetcher", "FilePath");
            set => _settingsIni.Write("Fetcher", "FilePath", value ?? "");
        }
        
        public int? Threads
        {
            get => int.TryParse(_settingsIni.Read("Fetcher", "Threads"), out var result) ? result : null;
            set => _settingsIni.Write("Fetcher", "Threads", value is null ? "True" : $"{value}");
        }
    }
}