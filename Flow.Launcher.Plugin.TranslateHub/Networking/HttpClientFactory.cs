using System.Net.Http;

namespace Flow.Launcher.Plugin.TranslateHub.Networking;

public static class HttpClientFactory
{
    private static readonly Lazy<HttpClient> DefaultClient = new(Create);

    public static HttpClient GetDefaultClient() => DefaultClient.Value;

    public static HttpClient Create()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(10),
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate,
            UseCookies = false,
            MaxConnectionsPerServer = 8
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }
}
