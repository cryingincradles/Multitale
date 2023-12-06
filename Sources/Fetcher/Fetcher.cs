using HDWallet.Bitcoin;
using HDWallet.Core;
using HDWallet.Ethereum;
using HDWallet.Tron;
using Multitale.Sources.Helpers;

namespace Multitale.Sources.Fetcher;

public static class Fetcher
{
    public class Total
    {
        public double ValueUSD { get; set; }
    }
    
    public static double? GetTotal<T>(MultiWallet.ChainWallet<T> chainWallet, Proxy? proxy = null) where T : IWallet, new()
    {
        double? networth = null;
        
        switch (chainWallet)
        {
            case MultiWallet.ChainWallet<TronWallet>:
                // Обработка для Tron
                //AnsiConsole.WriteLine($"Processing Tron chain: {chainWallet.Address}");
                networth = TronScan.GetNetworth(chainWallet.Address, proxy);
                break;
            case MultiWallet.ChainWallet<BitcoinWallet>:
                // Обработка для Bitcoin
                //AnsiConsole.WriteLine($"Processing Bitcoin chain: {chainWallet.Address}");
                break;
            case MultiWallet.ChainWallet<EthereumWallet>:
                // Обработка для Ethereum
                //AnsiConsole.WriteLine($"Processing Ethereum chain: {chainWallet.Address}");
                networth = DeBank.GetNetworth(chainWallet.Address, proxy);
                break;
        }

        return networth;
    }
}