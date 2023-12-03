using Multitale.Sources.Helpers;
using Newtonsoft.Json.Linq;

namespace Multitale.Sources.Fetcher;

public class DeBank
{
    public static string Url = "https://api.debank.com/user?id=";
    
    public static double? GetNetworth(string address, Proxy? proxy = null)
    {
        double? networth = null;

        var client = new HttpClient();
        if (proxy is not null)
        {
            client = new HttpClient(proxy.Data.Type!.Contains("http")
                ? new HttpClientHandler { Proxy = proxy.GetWebProxy(proxy.Data.Type) }
                : new SocketsHttpHandler { Proxy = proxy.GetWebProxy(proxy.Data.Type) });
            client.Timeout = proxy.Data.Timeout;
        }
        
        try
        {
            var response = client.GetAsync(Url + address).GetAwaiter().GetResult();
            
            if (response.IsSuccessStatusCode)
            {
                var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                var responseUsdValue = responseObject["data"]?["user"]?["desc"]?["usd_value"];
                
                if (responseUsdValue != null && double.TryParse(responseUsdValue.ToString(), out var parsedNetworth))
                    networth = parsedNetworth;
            }
        }
        catch
        {
            // ignored
        }

        return networth;
    }
}