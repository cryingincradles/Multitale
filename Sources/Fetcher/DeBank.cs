using System.Security.Cryptography;
using System.Text;
using Multitale.Sources.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Multitale.Sources.Fetcher;

public class DeBank
{
    private static readonly string Url = "https://api.debank.com/user";

    public static double? GetNetworth(string address, Proxy? proxy = null)
    {
        double? networth = -1;

        var client = new HttpClient();
        if (proxy is not null)
        {
            client = new HttpClient(proxy.Data.Type!.Contains("http")
                ? new HttpClientHandler { Proxy = proxy.GetWebProxy(proxy.Data.Type) }
                : new SocketsHttpHandler { Proxy = proxy.GetWebProxy(proxy.Data.Type) });
            client.Timeout = proxy.Data.Timeout;
        }

        var payload = new Dictionary<string, object>
        {
            { "id", address },
        };

        var bypassParams = new BypassParams("GET", Url.Replace("https://api.debank.com", ""), payload);

        client.DefaultRequestHeaders.Add("accept", "*/*");
        client.DefaultRequestHeaders.Add("accept-language", "ru,en;q=0.9,vi;q=0.8,es;q=0.7,cy;q=0.6");
        client.DefaultRequestHeaders.Add("origin", "https://debank.com");
        client.DefaultRequestHeaders.Add("referer", "https://debank.com");
        client.DefaultRequestHeaders.Add("source", "web");
        client.DefaultRequestHeaders.Add("x-api-ver", "v2");
        client.DefaultRequestHeaders.Add("account", bypassParams.AccountHeader);
        client.DefaultRequestHeaders.Add("x-api-nonce", bypassParams.Nonce);
        client.DefaultRequestHeaders.Add("x-api-sign", bypassParams.Signature);
        client.DefaultRequestHeaders.Add("x-api-ts", bypassParams.Timestamp);
        client.DefaultRequestHeaders.Add("user-agent", new Random().Next(0, 999999999).ToString());

        try
        {
            var response = client.GetAsync($"{Url}?id={address}").GetAwaiter().GetResult();
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            if (response.IsSuccessStatusCode)
            {
                var responseObject = JObject.Parse(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                var responseUsdValue = responseObject["data"]?["user"]?["desc"]?["usd_value"];

                if (responseUsdValue != null && double.TryParse(responseUsdValue.ToString(), out var parsedNetworth))
                    networth = parsedNetworth;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            // ignored
        }

        return networth;
    }

    private class BypassParams
    {
        public BypassParams() { }

        public BypassParams(string method, string path, Dictionary<string, object>? payload = null)
        {
            payload ??= new Dictionary<string, object>();
            var generatedParams = GenerateBypassParams(method, path, payload);
            AccountHeader = generatedParams.AccountHeader;
            Nonce = generatedParams.Nonce;
            Signature = generatedParams.Signature;
            Timestamp = generatedParams.Timestamp;
        }

        public string AccountHeader { get; set; } = null!;
        public string Nonce { get; set; } = null!;
        public string Signature { get; set; } = null!;
        public string Timestamp { get; set; } = null!;


        private static readonly Random Randomizer = new();
        private const string Chars = "abcdef0123456789";
        private const string Letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        private static string? GenerateRandomId()
        {
            var result = new char[32];

            for (var i = 0; i < result.Length; i++)
                result[i] = Chars[Randomizer.Next(Chars.Length)];

            return new string(result);
        }

        private static string SortQueryString(string queryString)
        {
            var queryParams = System.Web.HttpUtility.ParseQueryString(queryString);
            var sortedKeys = queryParams.AllKeys.OrderBy(k => k).ToList();
            var sortedQueryParams = sortedKeys.Select(key => $"{key}={queryParams[key]}");
            return string.Join("&", sortedQueryParams);
        }

        private static string CustomSha256(string data)
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static string GenerateNonce(int length)
        {
            var nonce = new char[length];
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[length];
            rng.GetBytes(randomBytes);

            for (int i = 0; i < length; i++)
                nonce[i] = Letters[randomBytes[i] % Letters.Length];

            return "n_" + new string(nonce);
        }

        private static string HmacSha256(byte[] key, byte[] data)
        {
            using var hmac = new HMACSHA256(key);
            var hashBytes = hmac.ComputeHash(data);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }

        private static string MapToQueryString(Dictionary<string, object> payload) => string.Join("&",
            payload.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value.ToString()!)}")
        );

        private static BypassParams GenerateBypassParams(string method,
            string path, Dictionary<string, object> payload)
        {
            var nonce = GenerateNonce(40);
            var queryString = MapToQueryString(payload);
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var randStr = $"debank-api\n{nonce}\n{timestamp}";
            var randStrHash = CustomSha256(randStr);

            var requestParams = $"{method.ToUpper()}\n{path.ToLower()}\n{SortQueryString(queryString)}";
            var requestParamsHash = CustomSha256(requestParams);

            var info = new Dictionary<string, object?>
            {
                { "random_at", timestamp },
                { "random_id", GenerateRandomId() },
                { "user_addr", null }
            };

            var accountHeader = JsonConvert.SerializeObject(info);
            var signature = HmacSha256(Encoding.UTF8.GetBytes(randStrHash), Encoding.UTF8.GetBytes(requestParamsHash));

            var result = new BypassParams
            {
                AccountHeader = accountHeader,
                Nonce = nonce,
                Signature = signature,
                Timestamp = timestamp.ToString()
            };

            return result;
        }
    }
}