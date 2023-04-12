#pragma warning disable CS0649

public class Structures
{
    public struct IniData
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public enum Types
    {
        Metamask,
        Binance,
        Ronin,
        Unknown
    }

    public struct Mnemonic
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public List<PrivateKey>? Child { get; set; }
    }

    public struct PrivateKeyData
    {
        public Mnemonic ParentSeed { get; set; }
        public int Account { get; set; }
    }

    public struct PrivateKey
    {
        public string Path { get; set; }
        public string Type { get; set; }
        public string Value { get; set; }
        public PrivateKeyData? Data { get; set; }
    }

    public struct Vault
    {
        public struct Regex
        {
            public string Vault { get; set; }
            public string Data { get; set; }
            public string IV { get; set; }
            public string Salt { get; set; }

        }

        public struct Encrypted
        {
            public string? Path { get; set; }
            public Structures.Types Type { get; set; }
            public string Data { get; set; }
            public string IV { get; set; }
            public string Salt { get; set; }
        }

        public struct Secret
        {
            public Structures.Types Type { get; set; }
            public string Password { get; set; }
            public List<Mnemonic> Mnemonics { get; set; }
            public List<PrivateKey> PrivateKeys { get; set; }
        }
    }

    public class PWallet
    {
        protected Vault.Regex _VaultRegex;

        protected Func<string, Vault.Secret>? _Parse;

        public PWallet(Vault.Regex VaultRegex, Func<string, Vault.Secret> Parse)
        {
            _Parse = Parse;
            _VaultRegex = VaultRegex;
        }

        public virtual Vault.Secret Parse(string decryptedData)
        {
            if (_Parse is null)
                return new Vault.Secret();
            return _Parse(decryptedData);
        }

        public virtual Vault.Regex VaultRegex()
        {
            return _VaultRegex;
        }
    }
}
