using static Structures;
using static Structures.Vault;

public class Recorders
{
    public class Universal
    {
        public static void RewriteDuplicates(string filePath)
        {
            SemaphoreSlim semaphore = new(0, 1);

            while (true)
            {
                try
                {
                    var Data = File.ReadAllText(filePath).Split("\n");
                    var NewData = string.Join("\n", Data.Distinct().ToList());

                    using FileStream fs = new(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    using var writer = new StreamWriter(fs);
                    writer.Write(NewData);
                    break;
                }
                catch (IOException)
                {
                    ThreadPool.QueueUserWorkItem(_ => semaphore.Wait());
                }
            }

            semaphore.Release();
        }

        public static void WriteValues(List<string> Values, string SavePath, Types type)
        {
            SemaphoreSlim semaphore = new(0, 1);
            var Settings = new Utils.IniFile("Settings.ini");
            bool WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            var PreviousDirectory = Path.GetDirectoryName(SavePath);

            if (PreviousDirectory is not null)
                Utils.TryCreateDirectory(PreviousDirectory);

            while (true)
            {
                try
                {
                    var UniqueValues = Values.Distinct().ToList();

                    if (!File.Exists(SavePath) && UniqueValues.Count > 0)
                        File.Create(SavePath).Close();

                    UniqueValues.ForEach(value =>
                    {
                        File.AppendAllText(SavePath, $"{value}\n");

                        if (WriteTypes is true)
                        {
                            Utils.TryCreateDirectory($"{Path.GetDirectoryName(SavePath)}/{type}");
                            File.AppendAllText($"{Path.GetDirectoryName(SavePath)}/{type}/{Path.GetFileName(SavePath)}", $"{value}\n");
                        };
                    });

                    break;
                }
                catch (IOException)
                {
                    ThreadPool.QueueUserWorkItem(_ => semaphore.Wait());
                }
            }

            semaphore.Release();

            RewriteDuplicates(SavePath);
            if (WriteTypes is true) RewriteDuplicates($"{Path.GetDirectoryName(SavePath)}/{type}/{Path.GetFileName(SavePath)}");
        }

        public static void WriteBalances(Balances.Total Balance, string SavePath)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            bool WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            var WBalancesPath = $"{SavePath}/Fetcher";

            if (!Directory.Exists(WBalancesPath))
                Utils.TryCreateDirectory(WBalancesPath);

            SemaphoreSlim semaphore = new(0, 1);
            while (true)
            {
                try
                {
                    string OutputText = $"Address: {Balance.Address}\nSecret: {(Balance.Mnemonic is null ? Balance.PrivateKey is null ? "MISSING" : Balance.PrivateKey.Value.Value : Balance.Mnemonic.Value.Value)}\nParsed: {Balance.Status}\nBalance: {Balance.Value}$\n\n";
                    File.AppendAllText($"{WBalancesPath}/All.txt", OutputText);
                    
                    if (WriteTypes is true)
                    {
                        if (Balance.Status is false) 
                            File.AppendAllText($"{WBalancesPath}/Errors.txt", OutputText);
                        if (Balance.Value > 0) 
                            File.AppendAllText($"{WBalancesPath}/Balances.txt", OutputText);
                        else 
                            File.AppendAllText($"{WBalancesPath}/Empty.txt", OutputText);
                    }
                }
                catch (IOException) { ThreadPool.QueueUserWorkItem(_ => semaphore.Wait()); }
                break;
            }
        }

        public static void WriteProxies(object ProxyObject, string SavePath)
        {
            var Settings = new Utils.IniFile("Settings.ini");
            bool WriteTypes = Settings.Read("Recorder", "WriteTypes") == "True";
            var WProxiesPath = $"{SavePath}/Proxy Scrapper";
            SemaphoreSlim semaphore = new(0, 1);

            while (true)
            {
                try
                {
                    List<Proxy> Proxies = new();

                    if (ProxyObject is List<Proxy> ProxyList)
                        Proxies = ProxyList;
                    
                    else if (ProxyObject is Proxy Proxy)
                        Proxies.Add(Proxy);

                    if (!Directory.Exists(WProxiesPath) && Proxies.Count > 0)
                        Utils.TryCreateDirectory(WProxiesPath);

                    foreach (var Proxy in Proxies)
                    {
                        bool IsValid = Proxy.Speed is not null;
                        string OutputText = $"{Proxy.IP}:{Proxy.Port}{(Proxy.Login is not null && Proxy.Password is not null ? $":{Proxy.Login}:{Proxy.Password}" : "")}\n";

                        if (WriteTypes is true)
                        {
                            if (IsValid is false)
                                File.AppendAllText($"{WProxiesPath}/Invalid.txt", OutputText);
                        }

                        if (IsValid is true)
                            File.AppendAllText($"{WProxiesPath}/Valid.txt", OutputText);
                    }
                }
                catch(IOException) { ThreadPool.QueueUserWorkItem(_ => semaphore.Wait()); }
                break;
            }

            semaphore.Release();
        }

        public static bool Record(string SavePath, object RecordObject)
        {
            try
            {
                if (RecordObject is Secret Secret)
                {
                    if (Secret.Mnemonics.Count > 0)
                        WriteValues(Secret.Mnemonics.Select(mnemonic => mnemonic.Value).ToList(), $"{SavePath}/Mnemonics.txt", Secret.Type);

                    if (Secret.PrivateKeys.Count > 0)
                        WriteValues(Secret.PrivateKeys.Select(key => key.Value).ToList(), $"{SavePath}/PrivateKeys.txt", Secret.Type);
                }

                else if (RecordObject is Balances.Total Balances)
                    WriteBalances(Balances, SavePath);

                else if (RecordObject is List<Proxy> ProxyList)
                    WriteProxies(ProxyList, SavePath);

                else if (RecordObject is Proxy Proxy)
                    WriteProxies(Proxy, SavePath);
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}