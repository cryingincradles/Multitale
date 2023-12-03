using System.Net;

namespace Multitale.Sources.Helpers;

public class Proxy
{
    private static string? _stringType;
    
    public ProxyObject Data;

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
        public string? Type => _stringType;
        public TimeSpan Timeout { get; }
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

    public Proxy(ProxyObject proxy, bool validate = false)
    {
        Data = proxy;
        if (validate)
            Validate();
    }

    public WebProxy GetWebProxy(string proxyType)
    {
        return new WebProxy
        {
            Address = new Uri($"{proxyType}://{Data.Ip}:{Data.Port}"),
            Credentials = Data.Login is null || Data.Password is null
                ? null
                : new NetworkCredential(Data.Login, Data.Password)
        };
    }
    
    // public HttpResponseMessage? Get(string requestUrl, int retries = 3)
    // {
    //     HttpResponseMessage? response = null;
    //
    //     for (var i = 0; i < retries; i++)
    //     {
    //         try
    //         {
    //             response = Data.Client?.GetAsync(requestUrl).GetAwaiter().GetResult();
    //             if (response is not null && response.IsSuccessStatusCode) 
    //                 break;
    //         }
    //
    //         catch
    //         {
    //             // ignored
    //         }
    //     }
    //
    //     return response;
    // }
    
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
        
        client.Timeout = Data.Timeout;
        
        try
        {
            response = client.GetAsync(requestUrl).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                if (_stringType != "socks5")
                    Validate();
                else
                {
                    Data.Status.Checked = true;
                    return;
                }
            }
            
            Data.Status.Checked = true;
            Data.Status.Valid = true;
        }
        
        catch (Exception)
        {
            if (_stringType != "socks5")
                Validate();

            else
                Data.Status.Checked = true;
        }
    }

    public static List<Proxy> GetProxyFromFile(string filePath)
    {
        var proxyList = new List<Proxy>();
        var lines = Utils.BufferedReadLines(filePath);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            var splittedLine = trimmedLine.Split(":");
            
            if (splittedLine.Length <= 1)
                continue;
            
            string ip;
            int port;
            string? login = null;
            string? password = null;

            ip = splittedLine[0];
            var portPase = int.TryParse(splittedLine[1], out var parsedPort);
            
            if (!portPase) 
                continue;

            port = parsedPort;
            if (splittedLine.Length > 3)
            {
                login = splittedLine[2];
                password = splittedLine[3];
            }
            
            proxyList.Add(new Proxy(new ProxyObject(ip, port, login, password)));
        }

        return proxyList;
    }
}