using System.Net;
using System.Net.Sockets;
using PactSample.Users.PactTests;
using Serilog;

public class ProviderStates(IDictionary<string, Action<IServiceProvider>> states)
{
    public readonly IDictionary<string, Action<IServiceProvider>> States = states;
}
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

    protected abstract IDictionary<string, Action<IServiceProvider>> GetProviderStates();

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
                var logDir = Path.Combine(Directory.GetCurrentDirectory(), "pact-debug");
                Directory.CreateDirectory(logDir); // Ensure directory exists

                var logPath = Path.Combine(logDir, "pact-provider-debug.log");
                
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                    .CreateLogger();

                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseUrls(serverUri.ToString());
                        webBuilder.UseStartup<T>();
                        ConfigureTestWebHost(webBuilder);
                    }).UseSerilog();

                hostBuilder.ConfigureServices(services =>
                {
                    ConfigureTestServices(services);
                    // Register middleware injection
                    services.AddSingleton<IStartupFilter, TestMiddlewareStartupFilter>();
                    services.AddSingleton(new ProviderStates(GetProviderStates()));
                });
                
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


public class TestMiddlewareStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            // Add your test middleware first
            app.UseMiddleware<BaseProviderStateMiddleware>();
            // Then call the rest of the pipeline
            next(app);
        };
    }
}