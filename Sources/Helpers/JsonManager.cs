using Newtonsoft.Json;

namespace Multitale.Sources.Helpers;

public class JsonManager<T>
{
    private readonly T _defaultItem;
    private readonly string _defaultItemName;
    private readonly string _itemsPath;
    private readonly object _fileLocker = new();
    private readonly string _managerName;
    
    private readonly Dictionary<string, T> _cache = new();

    protected JsonManager(string itemsPath, T defaultItem, string managerName)
    {
        _defaultItem = defaultItem;
        _defaultItemName = defaultItem!.GetType().Name;
        _itemsPath = itemsPath;
        _managerName = managerName;
    }

    protected static T? GetFromFile(string file)
    {
        var jsonString = File.ReadAllText(file);
        var item = JsonConvert.DeserializeObject<T>(jsonString);
        return item;
    }

    protected T Get(string itemName, bool swapEmptyProps = false)
    {
        itemName += ".json";
        string? jsonString;

        if (_cache.TryGetValue(itemName, out var cachedItem))
            return cachedItem;

        try
        {
            jsonString = File.ReadAllText(Path.Combine(_itemsPath, itemName));
        }
        
        catch (Exception ex)
        {
            if (itemName != _defaultItemName)
            {
                Program.Log.Error($"Error occurred while trying to read {itemName} file\n{ex}");
                Program.Log.Warning($"Default {_managerName} will be used");
            }

            if (!_cache.ContainsKey(_defaultItemName))
                CreateDefaultItem();
            return _cache[_defaultItemName];
        }

        var item = JsonConvert.DeserializeObject<T>(jsonString);
        if (item is null)
            Program.Log.Warning($"Loaded {_managerName} file ({itemName}) structure is different from basic structure. Default \"{_defaultItemName}\" {_managerName} will be used");
        else
        {
            var typeProps = typeof(T).GetProperties();
            var itemProps = typeProps.Select(el => el.GetValue(item));
            var hasNull = itemProps.Any(prop => prop is null);

            if (hasNull)
            {
                if (swapEmptyProps)
                {
                    foreach (var property in typeof(T).GetProperties())
                    {
                        var value = property.GetValue(item);
                        if (value is not null) continue;
                
                
                        var defaultValue = property.GetValue(_defaultItem);
                        property.SetValue(item, defaultValue);
                    }
                }

                else
                    item = default;
            }
        }
        
        if (item is not null)
            _cache.Add(itemName, item);
        
        return item ?? _cache[_defaultItemName];
    }

    protected void Load(T? additionalItem = default)
    {
        Program.Log.Information($"Checking default {_managerName}(-s)");
        CreateDefaultItem();
        CreateDefaultItem(additionalItem);
    }

    protected void Load(List<T> additionalItems)
    {
        Program.Log.Information($"Checking default {_managerName}(-s)");
        CreateDefaultItem();
        additionalItems.ForEach(CreateDefaultItem);
    }

    private void CreateDefaultItem(T? additionalItem = default)
    {
        lock (_fileLocker)
        {
            var item = additionalItem ?? _defaultItem!;
            var itemName = item.GetType().Name;

            if (!Directory.Exists(_itemsPath))
                Directory.CreateDirectory(_itemsPath);

            var itemFilePath = Path.Combine(_itemsPath, $"{itemName}.json");

            if (!_cache.ContainsKey(itemName))
            {
                Program.Log.Information($"Saving {itemName} {_managerName} to program cache...");
                _cache.Add(itemName, item);
            }

            if (File.Exists(itemFilePath)) return;

            Program.Log.Information($"Creating {itemName} {_managerName} file...");

            var defaultItem = _cache[itemName];
            var defaultItemString = JsonConvert.SerializeObject(defaultItem, Formatting.Indented);

            File.WriteAllText(itemFilePath, defaultItemString);
        }
    }
}