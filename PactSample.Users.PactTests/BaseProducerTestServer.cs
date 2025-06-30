using System.Net;
using System.Net.Sockets;
using PactSample.Users.PactTests;

public abstract class BaseProducerTestServer<T> : IDisposable where T : class 
{
    private readonly IHost _server;
    public Uri ServerUri { get; }

    protected BaseProducerTestServer()
    {
        var (host, uri) = StartServer();
        _server = host;
        ServerUri = uri;
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        
    }

    protected virtual void ConfigureTestWebHost(IWebHostBuilder webHostBuilder)
    {
        
    }

    private (IHost server, Uri uri) StartServer()
    {
        const int maxRetries = 5;
        Exception? lastException = null;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            var port = GetFreeTcpPort();
            var serverUri = new Uri($"http://{IPAddress.Loopback}:{port}");

            try
            {
                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls(serverUri.ToString());
                        webBuilder.UseStartup<T>();
                        ConfigureTestWebHost(webBuilder);
                        webBuilder.Configure(app =>
                        {
                            app.UseMiddleware<BaseProviderStateMiddleware>();
                        });
                    });

                hostBuilder.ConfigureServices(ConfigureTestServices);
                var server = hostBuilder.Build();
                
                server.Start(); // May throw if port is already taken
                return (server, serverUri);
            }
            catch (Exception ex) when (ex is SocketException || ex.InnerException is SocketException)
            {
                lastException = ex;
            }
        }

        throw new InvalidOperationException($"Failed to start server after {maxRetries} attempts.", lastException);
    }
    
    public void Dispose()
    {
        _server.Dispose();
    }
    
    private static int GetFreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        
        l.Stop();
        return port;
    }
}