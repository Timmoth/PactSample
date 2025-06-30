using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace PactSample.Users.PactTests;

public abstract class BaseProviderStateMiddleware(RequestDelegate next)
{
    protected abstract IDictionary<string, Action> ProviderStates { get; }

    public Task Invoke(HttpContext context)
    {
        if (context.Request.Path.Value != "/provider-states")
            return next(context);
        
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        if (context.Request.Method != HttpMethod.Post.ToString() || context.Request.Body == null)
            return next(context);
        
        string jsonRequestBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
        {
            jsonRequestBody = reader.ReadToEnd();
        }

        var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

        //A null or empty provider state key must be handled
        if (!string.IsNullOrEmpty(providerState?.State) &&
            ProviderStates.TryGetValue(providerState.State, out var action))
        {
            action();
        }
        else
        {
            Console.WriteLine($"Unknown provider state: '{providerState?.State}'");
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return context.Response.WriteAsync($"Unknown provider state: '{providerState?.State}'");
        }


        return context.Response.WriteAsync(string.Empty);


    }
   
}

public class ProviderState
{
    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("consumer")]
    public string Consumer { get; set; }
}