using System.Net;

namespace Multitale.Sources.Helpers;

public class Proxy
{
    private static string? _stringType;
    
    public ProxyObject LoadedProxy;

    public class StatusObject
    {
        public bool Checked { get; set; }
        public bool Valid { get; set; }
    }
    
    public class ProxyObject
    {
        public string Ip { get; }
        public int Port { get; }
        public string? Login { get; set; }
        public string? Password { get; set; }
        public TimeSpan Timeout { get; }
        public HttpClient? Client { get; set; }
        public StatusObject Status { get; }

        public ProxyObject(string ip, int port, string? login = null, string? password = null, int timeoutMs = 5000)
        {
            Ip = ip;
            Port = port;
            Login = login;
            Password = password;
            Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            Status = new StatusObject();
        }
    }

    public Proxy(ProxyObject loadedProxy)
    {
        LoadedProxy = loadedProxy;
    }

    private WebProxy GetWebProxy(string proxyType)
    {
        return new WebProxy
        {
            Address = new Uri($"{proxyType}://{LoadedProxy.Ip}:{LoadedProxy.Port}"),
            Credentials = LoadedProxy.Login is null || LoadedProxy.Password is null
                ? null
                : new NetworkCredential(LoadedProxy.Login, LoadedProxy.Password)
        };
    }
    
    public HttpResponseMessage? Get(string requestUrl, int retries = 3)
    {
        HttpResponseMessage? response = null;

        for (var i = 0; i < retries; i++)
        {
            try
            {
                response = LoadedProxy.Client?.GetAsync(requestUrl).GetAwaiter().GetResult();
                if (response is not null && response.IsSuccessStatusCode) 
                    break;
            }

            catch
            {
                // ignored
            }
        }

        return response;
    }
    
    public void Validate()
    {
        var requestUrl = "https://httpbin.org/get";
        HttpClient client = new();
        SocketsHttpHandler socketsHandler;
        HttpResponseMessage? response;

        switch (_stringType)
        {
            case null:
                _stringType = "http";
                var httpHandler = new HttpClientHandler { Proxy = GetWebProxy(_stringType) };
                client = new HttpClient(httpHandler);
                break;
            
            case "http":
                _stringType = "socks4";
                socketsHandler = new SocketsHttpHandler { Proxy = GetWebProxy(_stringType) };
                client = new HttpClient(socketsHandler);
                break;
            
            case "socks4":
                _stringType = "socks4a";
                socketsHandler = new SocketsHttpHandler { Proxy = GetWebProxy(_stringType) };
                client = new HttpClient(socketsHandler);
                break;
            
            case "socks4a":
                _stringType = "socks5";
                socketsHandler = new SocketsHttpHandler { Proxy = GetWebProxy(_stringType) };
                client = new HttpClient(socketsHandler);
                break;
        }
        
        client.Timeout = LoadedProxy.Timeout;
        
        try
        {
            response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                if (_stringType != "socks5")
                    Validate();
                else
                {
                    LoadedProxy.Status.Checked = true;
                    return;
                }
            }
            
            LoadedProxy.Status.Checked = true;
            LoadedProxy.Status.Valid = true;
            LoadedProxy.Client = client;
        }
        
        catch (Exception)
        {
            if (_stringType != "socks5")
                Validate();

            else
                LoadedProxy.Status.Checked = true;
        }
    }
}