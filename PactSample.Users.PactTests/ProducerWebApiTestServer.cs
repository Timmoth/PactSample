public class ProducerWebApiTestServer : IDisposable
{
    private readonly WebApplication _webApp;
    public Uri ServerUri { get; }

    public ProducerWebApiTestServer()
    {
        ServerUri = new Uri("http://localhost:9223");
        // Setup app like in Program.cs
        _webApp = Program.CreateApp();

        // Override the URL binding
        _webApp.Urls.Clear();
        _webApp.Urls.Add(ServerUri.ToString());
        
        // Start the app without blocking
        _webApp.Start();
  
    }

    public void Dispose()
    {
        _ = _webApp.StopAsync();
        _ = _webApp.DisposeAsync();
    }
}