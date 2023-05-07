public class Structures
{
    public static Dictionary<string, Chains> DeBankChains = new()
    {
        { "ethereum", Chains.Ethereum },
        { "bnb chain", Chains.BinanceSC },
        { "polygon", Chains.Polygon },
        { "avanalnche", Chains.Avalanche },
        { "arbitrum", Chains.Arbitrum },
        { "arbitrum nova", Chains.ArbitrumNova },
        { "gnosis chain", Chains.Gnosis },
        { "fantom", Chains.Fantom },
        { "boba", Chains.Boba },
        { "okc", Chains.OKC },
        { "heco", Chains.Heco },
        { "optimism", Chains.Optimism },
        { "celo", Chains.Celo },
        { "moonriver", Chains.Moonriver },
        { "cronos", Chains.Cronos },
        { "metis", Chains.Metis },
        { "bttc", Chains.BTTC },
        { "aurora", Chains.Aurora },
        { "moonbeam", Chains.Moonbeam },
        { "smartbch", Chains.SmartBCH },
        { "harmony", Chains.Harmony },
        { "fuse", Chains.Fuse },
        { "astar", Chains.Astar },
        { "palm", Chains.Palm },
        { "shiden", Chains.Shiden },
        { "klaytn", Chains.Klaytn },
        { "rsk", Chains.RSK },
        { "iotex", Chains.Iotex },
        { "kcc", Chains.KCC },
        { "wanchain", Chains.Wanchain },
        { "songbird", Chains.Songbird },
        { "evmos", Chains.Evmos },
        { "dfk", Chains.DFK },
        { "telos", Chains.Telos },
        { "swimmer", Chains.Swimmer },
        { "canto", Chains.Canto },
        { "dogechain", Chains.Dogechain },
        { "step", Chains.Step },
        { "kava", Chains.Kava },
        { "milkomeda", Chains.Milkomeda },
        { "conflux", Chains.Conflux },
        { "bitgert", Chains.Bitgert },
        { "godwoken", Chains.Godwoken },
        { "tomb chain", Chains.Tomb },
        { "polygon zkevm", Chains.PolygonZK },
        { "zksync era", Chains.zkSync },
        { "eos evm", Chains.EOS },
        { "core", Chains.Core }
    };

    public struct Balances
    {
        public struct NFT
        {
            public Chains Chain { get; set; }
            public string Name { get; set; }
            public Price LastOffer { get; set; }
            public Price BestOffer { get; set; }
            public int Amount { get; set; }
        }

        public struct Token
        {
            public Chains Chain { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
            public double Amount { get; set; }
        }

        public struct Chain
        {
            public Chains Name { get; set; }
            public Default Balances { get; set; }
        }

        public struct Default
        {
            public double Value { get; set; }
            public List<Pool>? Pools { get; set; }
            public List<Token>? Tokens { get; set; }
            public List<NFT>? NFTs { get; set; }
        }

        public struct Price
        {
            public double Value { get; set; }
            public double Amount { get; set; }
        }

        public struct Pool
        {
            public Chains Chain { get; set; }
            public string Name { get; set; }
            public double Value { get; set; }
        }

        public struct Total
        {
            public string Address { get; set; }
            public Mnemonic? Mnemonic { get; set; }
            public PrivateKey? PrivateKey { get; set; }
            public double Value { get; set; }
            public List<Chain>? Chains { get; set; }
            public bool Status { get; set; }
        }
    }

    public struct IniData
    {
        public string Section { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public enum Chains
    {
        Ethereum,
        BinanceSC,
        BinanceC,
        Polygon,
        Fantom,
        Arbitrum,
        ArbitrumNova,
        Optimism,
        Gnosis,
        Avalanche,
        Moonriver,
        Moonbeam,
        Metis,
        BTTC,
        Astar,
        Shiden,
        RSK,
        KCC,
        Core,
        DFK,
        Swimmer,
        Dogechain,
        Step,
        Kava,
        Milkomeda,
        Conflux,
        Bitgert,
        Godwoken,
        Tomb,
        EOS,
        Iotex,
        Wanchain,
        Evmos,
        Aurora,
        SmartBCH,
        Fuse,
        zkSync,
        Klaytn,
        Canto,
        Boba,
        OKC,
        Heco,
        Harmony,
        Telos,
        Celo,
        Cronos,
        Palm,
        Songbird,
        PolygonZK,
        Bitcoin,
        Litecoin,
        Dogecoin
    }

    public enum Types
    {
        Metamask,
        Binance,
        Ronin,
        Unknown
    }

    public enum StringWalletTypes
    {
        PrivateKey,
        Mnemonic,
        JSON
    }

    public enum ProxyType
    {
        HTTPS,
        HTTP,
        SOCKS4,
        SOCKS5
    }

    public enum Functions
    {
        Fetcher,
        Decryptor,
        ProxyScrapper,
        ProxyValidator
    }

    public struct StringWallet
    {
        public StringWalletTypes Type;
        public string Value;
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

    public struct StringSecret
    {
        public List<string> Mnemonics { get; set; }
        public List<string> PrivateKeys { get; set; }
    }

    public struct Proxy
    {
        public string? Source { get; set; }
        public string IP { get; set; }
        public int Port { get; set; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public int? Speed { get; set; }
        public ProxyType? Type { get; set; }
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
            public Types Type { get; set; }
            public string Data { get; set; }
            public string IV { get; set; }
            public string Salt { get; set; }
        }

        public struct Secret
        {
            public Types Type { get; set; }
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