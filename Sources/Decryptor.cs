using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Pastel;
using System.Text;
using System.Text.RegularExpressions;

public class Decryptor
{
    public static Structures.Types GetVaultType(string path)
    {
        string? pathName = Path.GetDirectoryName(path);

        if (pathName is not null)
        {
            Structures.Types type = pathName switch
            {
                string s when s.Length < 2 => Structures.Types.Unknown,
                string s when s.Contains("Binance") => Structures.Types.Binance,
                string s when s.Contains("Ronin") => Structures.Types.Ronin,
                string s when s.Contains("Metamask") => Structures.Types.Metamask,
                _ => Structures.Types.Unknown,
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
                    encryptedVault.Path?.Length < 8) continue;
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
                Console.WriteLine(DecryptedString);

                if (DecryptedString is null)
                {
                    if (Password == passwords[^1])
                    {
                        DateTime TryTime = DateTime.Now;
                        Console.WriteLine($"[{TryTime:H:mm:ss}]\t{"Path:".Pastel(ConsoleColor.Yellow)} {EncryptedVault.Path}\n\t\t{("Data:".Pastel(ConsoleColor.Yellow))} {EncryptedVault.Data}\n\t\t{("IV:".Pastel(ConsoleColor.Yellow))} {EncryptedVault.IV}\n\t\t{("Salt:".Pastel(ConsoleColor.Yellow))} {EncryptedVault.Salt}\n\t\t{("MSG:".Pastel(ConsoleColor.Yellow))} {("No valid password found".Pastel(ConsoleColor.Red))}\n");
                    }
                }

                else
                {
                    Secret.Password = Password;
                    Secret.Type = EncryptedVault.Type;

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

        if (CachedPassword is null) return null;

        DateTime CurrentTime = DateTime.Now;
        string OutputTime = $"[{CurrentTime:H:mm:ss}]";
        string OutputPath = $"{"Path:".Pastel(ConsoleColor.Yellow)} {path}";
        string OutputType = $"{"Type:".Pastel(ConsoleColor.Yellow)} {CachedType}";
        string OutputMnemonics = $"{$"Phrases ({Secret.Mnemonics.Count}):".Pastel(ConsoleColor.Yellow)} {(Secret.Mnemonics.Count == 0 ? "None" : string.Join($"\n\t\t\t{string.Concat(Enumerable.Repeat(" ", Secret.Mnemonics.Count.ToString().Length))}    ", Secret.Mnemonics.Select(el => el.Value).ToArray()))}";
        string OutputKeys = $"{$"Keys ({Secret.PrivateKeys.Count}):".Pastel(ConsoleColor.Yellow)} {(Secret.PrivateKeys.Count == 0 ? "None" : string.Join($"\n\t\t\t{string.Concat(Enumerable.Repeat(" ", Secret.PrivateKeys.Count.ToString().Length))} ", Secret.PrivateKeys.Select(el => el.Value).ToArray()))}";
        string OutputMSG = $"{"MSG:".Pastel(ConsoleColor.Yellow)} {$"Success with password {CachedPassword}".Pastel(ConsoleColor.Green)}";

        // Console output
        Console.WriteLine($"{OutputTime}\t{OutputPath}\n\t\t{OutputType}\n\t\t{OutputMnemonics}\n\t\t{OutputKeys}\n\t\t{OutputMSG}\n");

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
}
