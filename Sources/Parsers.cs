#pragma warning disable CS8604
#pragma warning disable CS8602

using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static Structures;
using static Structures.Vault;

public class Parsers
{
    public class Metamask : PWallet
    {
        private new static Vault.Regex _VaultRegex = new()
        {
            Vault = "\"data.*?}",
            Data = "\"data\":\"(.*?)\"",
            IV = "\"iv\":\"(.*?)\"",
            Salt = "\"salt\":\"(.*?)\""
        };

        private static Func<string, Vault.Secret> _ParseFunc = (decryptedString) => 
        {
            var Secret = new Secret();

            try
            {
                Secret.Mnemonics = new();
                Secret.PrivateKeys = new();

                JArray DataObject = JArray.Parse(decryptedString);

                foreach (var Object in DataObject)
                {
                    string ObjectType = Object["type"].ToString();

                    if (ObjectType.Contains("HD"))
                    {
                        var ObjectMnemonic = Object["data"]["mnemonic"];
                        Structures.Mnemonic Mnemonic = new() { Type = "Default" };

                        if (ObjectMnemonic.GetType() == typeof(JArray) && ObjectMnemonic.Count() > 1)
                            Mnemonic.Value = Encoding.Default.GetString(ObjectMnemonic.ToObject<byte[]>());

                        else
                            Mnemonic.Value = ObjectMnemonic.ToString();

                        if (!Secret.Mnemonics.Contains(Mnemonic))
                            Secret.Mnemonics.Add(Mnemonic);

                        if ((int)Object["data"]["numberOfAccounts"] > 1)
                        {
                            for (int i = 1; i < (int)Object["data"]["numberOfAccounts"]; i++)
                            {
                                Secret.PrivateKeys.Add(new Structures.PrivateKey()
                                {
                                    Type = "Default",
                                    Value = new Nethereum.HdWallet.Wallet(Mnemonic.Value, null).GetAccount(i).PrivateKey,
                                    Data = new()
                                    {
                                        Account = i + 1,
                                        ParentSeed = Mnemonic
                                    }
                                });
                            }
                        }
                    }

                    else if (ObjectType.Contains("Pair"))
                    {
                        foreach (var ObjectKey in Object["data"])
                        {
                            Secret.PrivateKeys.Add(new Structures.PrivateKey() { Type = "Default", Value = ObjectKey.ToString() });
                        }
                    }

                    else continue;
                }

                return Secret;
            }

            catch (Exception)
            {
                return Secret;
            }
        };

        public Metamask() : base(_VaultRegex, _ParseFunc) { }
    }

    public class Ronin : PWallet
    {
        private static new readonly Vault.Regex _VaultRegex = new()
        {
            Vault = "\"data.*?}",
            Data = "\"data\":\"(.*?)\"",
            IV = "\"iv\":\"(.*?)\"",
            Salt = "\"salt\":\"(.*?)\""
        };

        private static Func<string, Vault.Secret> _ParseFunc = (decryptedString) =>
        {
            var Secret = new Secret();

            try
            {
                Secret.Mnemonics = new();
                Secret.PrivateKeys = new();
                decryptedString = decryptedString.Substring(1, decryptedString.Length - 2).Replace("\\", "");

                if (decryptedString.StartsWith("0x")) Secret.PrivateKeys.Add(new() { Type = "Default", Value = decryptedString });

                else
                {
                    JObject DataObject = JObject.Parse(decryptedString);

                    if (((int)DataObject["totalAccount"]) == 1) Secret.Mnemonics.Add(new() { Type = "Default", Value = DataObject["mnemonic"].ToString() });

                    else
                    {
                        string SMnemonic = DataObject["mnemonic"].ToString();
                        string SPrivateKey = new Nethereum.HdWallet.Wallet(SMnemonic, null).GetAccount((int)DataObject["totalAccount"]).PrivateKey;
                        Structures.Mnemonic Mnemonic = new() { Type = "Default", Value = SMnemonic };
                        Structures.PrivateKey PrivateKey = new()
                        {
                            Type = "Default",
                            Value = SPrivateKey,
                            Data = new()
                            {
                                Account = (int)DataObject["totalAccount"],
                                ParentSeed = Mnemonic
                            }
                        };
                        Secret.PrivateKeys.Add(PrivateKey);
                    }
                }

                return Secret;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return Secret;
            }
        };

        public Ronin() : base(_VaultRegex, _ParseFunc) { }
    }

    public class Binance : PWallet
    {
        private static new readonly Vault.Regex _VaultRegex = new()
        {
            Vault = "\"data.*?}",
            Data = "\"data\":\"(.*?)\"",
            IV = "\"iv\":\"(.*?)\"",
            Salt = "\"salt\":\"(.*?)\""
        };

        private static Func<string, Vault.Secret> _ParseFunc = (decryptedString) =>
        {
            var Secret = new Secret();

            try
            {
                Secret.Mnemonics = new();
                Secret.PrivateKeys = new();

                JObject DataObject = JObject.Parse(decryptedString);

                if (DataObject["accounts"].Count() > 0)
                {
                    foreach (var Object in DataObject["accounts"])
                    {
                        string ObjectType = Object["type"].ToString();

                        if (ObjectType.Contains("local"))
                        {
                            Secret.Mnemonics.Add(new Structures.Mnemonic() { Type = "BBC", Value = Object["mnemonic"].ToString().Trim() });
                        }

                        if (Object["addresses"].Count() > 1)
                        {
                            foreach (var AddressObject in Object["addresses"])
                            {
                                string AddressType = AddressObject["type"].ToString();

                                if (AddressType.Contains("test")) continue;
                                if (AddressType.Contains("eth")) AddressType = "Default";
                                if (ObjectType == "imported") AddressType = "BBC";
                                else AddressType = "BBC";

                                Structures.PrivateKey PrivateKey = new() { Type = AddressType, Value = AddressObject["privateKey"].ToString().Trim() };
                                if (!Secret.PrivateKeys.Contains(PrivateKey)) Secret.PrivateKeys.Add(PrivateKey);
                            }
                        }
                    }
                }
            }

            catch (Exception)
            {
                return Secret;
            }

            Secret.PrivateKeys = Secret.PrivateKeys.GroupBy(element => new { element.Value })
                      .Select(element => element.First())
                      .ToList();

            Secret.Mnemonics = Secret.Mnemonics.GroupBy(element => new { element.Value })
                      .Select(element => element.First())
                      .ToList();

            return Secret;
        };

        public Binance() : base(_VaultRegex, _ParseFunc) { }
    }

    public class Unknown : PWallet
    {
        private static new readonly Vault.Regex _VaultRegex = new()
        {
            Vault = "\"data.*?}",
            Data = "\"data\":\"(.*?)\"",
            IV = "\"iv\":\"(.*?)\"",
            Salt = "\"salt\":\"(.*?)\""
        };

        private static readonly Func<string, Vault.Secret> _ParseFunc = (decryptedString) =>
        {
            var Secret = new Secret();

            try
            {
                Secret.Mnemonics = new();
                Secret.PrivateKeys = new();

                var MnemonicExp = new System.Text.RegularExpressions.Regex("^([a-z]+ ){11}[a-z]+$|^([a-z]+ ){14}[a-z]+$|^([a-z]+ ){17}[a-z]+$|^([a-z]+ ){20}[a-z]+$|^([a-z]+ ){23}[a-z]+$");
            }

            catch (Exception)
            {
                return Secret;
            }

            Secret.PrivateKeys = Secret.PrivateKeys.GroupBy(element => new { element.Value })
                      .Select(element => element.First())
                      .ToList();

            Secret.Mnemonics = Secret.Mnemonics.GroupBy(element => new { element.Value })
                      .Select(element => element.First())
                      .ToList();

            return Secret;
        };

        public Unknown() : base(_VaultRegex, _ParseFunc) { }
    }

    public static Dictionary<Types, PWallet> SupportedWallets = new()
    {
        { Types.Metamask, new Metamask() },
        { Types.Ronin, new Ronin() },
        { Types.Binance, new Binance() },
        { Types.Unknown, new Unknown() }
    };

    public static Vault.Secret Parse(Types type, string decryptedString)
    {
        var Secret = new Secret();

        try
        {
            Secret = SupportedWallets[type].Parse(decryptedString);
        }

        catch(Exception)
        {
            return Secret;
        }

        return Secret;
    }
}
