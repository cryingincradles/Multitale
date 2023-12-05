#pragma warning disable

using System.Text.RegularExpressions;
using HDWallet.Bitcoin;
using HDWallet.Core;
using HDWallet.Ethereum;
using HDWallet.Tron;
using Nethereum.Web3.Accounts;

namespace Multitale.Sources.Helpers;

public class MultiWallet
{
    public class Mnemonic
    {
        public string Data { get; }
        public Mnemonic(string data) => Data = data;
    }
    
    public class PrivateKey
    {
        public string Data { get; }
        public PrivateKey(string data) => Data = data;
    }
    
    public class ChainWallet<T> where T : IWallet, new()
    {
        private double? _cachedNetworth;

        public string Address { get; set; }
        public T Account { get; set; }
        public IHDWallet<T>? Wallet { get; set; }
        public double? Networth => _cachedNetworth;
        public double? GetNetworth(Proxy? proxy = null)
        {
            var networth = Fetcher.Fetcher.GetTotal(this, proxy);
            if (_cachedNetworth is null && networth is not null)
            {
                networth = Math.Round((double)networth, 2);
                _cachedNetworth = networth;
            }
            
            return networth;
        }
    }
    
    public class Wallet
    {
        public Mnemonic? Mnemonic { get; set; }
        public PrivateKey? PrivateKey { get; set; }
        public ChainWallet<TronWallet> Tron { get; set; }
        public ChainWallet<BitcoinWallet> Bitcoin { get; set; }
        public ChainWallet<EthereumWallet> Ethereum { get; set; }

        public Wallet(Mnemonic mnemonic)
        {
            Mnemonic = mnemonic;
            
            Ethereum = new ChainWallet<EthereumWallet>();
            Ethereum.Wallet = new EthereumHDWallet(mnemonic.Data, "");
            Ethereum.Account = Ethereum.Wallet.GetAccountWallet(0);
            Ethereum.Address = Ethereum.Account.Address;
            
            Tron = new ChainWallet<TronWallet>();
            Tron.Wallet = new TronHDWallet(mnemonic.Data);
            Tron.Account = Tron.Wallet.GetAccountWallet(0);
            Tron.Address = Tron.Account.Address;
            
            Bitcoin = new ChainWallet<BitcoinWallet>();
            Bitcoin.Wallet = new BitcoinHDWallet(mnemonic.Data, "");
            Bitcoin.Account = Bitcoin.Wallet.GetAccountWallet(0);
            Bitcoin.Address = Bitcoin.Account.Address;
        }

        public Wallet(PrivateKey privateKey)
        {
            PrivateKey = privateKey;
            
            Ethereum = new ChainWallet<EthereumWallet>();
            Ethereum.Account = new EthereumWallet(privateKey.Data);
            Ethereum.Address = Ethereum.Account.Address;
            
            Tron = new ChainWallet<TronWallet>();
            Tron.Account = new TronWallet(privateKey.Data);
            Tron.Address = Tron.Account.Address;
            
            Bitcoin = new ChainWallet<BitcoinWallet>();
            Bitcoin.Account = new BitcoinWallet(privateKey.Data);
            Bitcoin.Address = Bitcoin.Account.Address;
        }
    }

    public static List<Wallet> GetWalletsFromFile(string filePath)
    {
        var wallets = new List<Wallet>();
        var lines = Utils.BufferedReadLines(filePath)
            .Select(line => line.Trim());

        var mnemonicEx = new string("([a-z]+ ){11}[a-z]+$|^([a-z]+ ){14}[a-z]+$|^([a-z]+ ){17}[a-z]+$|^([a-z]+ ){20}[a-z]+$|^([a-z]+ ){23}[a-z]+");
        var mnemonicRegEx = new Regex(mnemonicEx, RegexOptions.Multiline);
            
        var privateKeyEx = new string("^(0x)?[0-9a-fA-F]{64}$");
        var privateKeyRegEx = new Regex(privateKeyEx, RegexOptions.Multiline);
        
        foreach (var line in lines)
        {
            var mnemonicMatch = mnemonicRegEx.Match(line);
            var privateKeyMatch = privateKeyRegEx.Match(line);

            if (mnemonicMatch.Success)
            {
                if (!IsValidMnemonic(mnemonicMatch.Value)) continue;
                
                var lineWallet = new Wallet(new Mnemonic(line));
                wallets.Add(lineWallet);

            }
            
            else if (privateKeyMatch.Success)
            {
                if (!IsValidPrivateKey(privateKeyMatch.Value)) continue;
                
                var lineWallet = new Wallet(new PrivateKey(line));
                wallets.Add(lineWallet);
            }
        }

        return wallets.DistinctBy(wallet => wallet.Ethereum.Address).ToList();
    }

    public static bool IsValidMnemonic(string mnemonic)
    {
        try
        {
            _ = new Nethereum.HdWallet.Wallet(mnemonic, "");
            return true;
        }

        catch
        {
            return false;
        }
    }
    
    public static bool IsValidPrivateKey(string privateKey)
    {
        try
        {
            _ = new Account(privateKey);
            return true;
        }

        catch
        {
            return false;
        }
    }
}