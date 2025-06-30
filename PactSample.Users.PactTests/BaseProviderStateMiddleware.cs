using System.Net;
using System.Text;
using System.Text.Json;

namespace PactSample.Users.PactTests;

public abstract class BaseProviderStateMiddleware(RequestDelegate next)
{ 
    protected abstract IDictionary<string, Action> ProviderStates { get; }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.Value == "/provider-states")
        {
            this.HandleProviderStatesRequest(context);
            await context.Response.WriteAsync(String.Empty);
        }
        else
        {
            await next(context);
        }
    }

    private void HandleProviderStatesRequest(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        
        if (context.Request.Method.ToUpper() == HttpMethod.Post.ToString().ToUpper() &&
            context.Request.Body != null)
        {
            string jsonRequestBody = String.Empty;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
            {
                jsonRequestBody = reader.ReadToEnd();
            }

            var providerState = JsonSerializer.Deserialize<ProviderState>(jsonRequestBody);

            //A null or empty provider state key must be handled
            if (providerState != null && !String.IsNullOrEmpty(providerState.State))
            {
                if(ProviderStates.TryGetValue(providerState.State, out var acc))
                {
                    acc?.Invoke();
                }
            }
        }
    }
}

public class ProviderState
{
    public string Consumer { get; set; }
    public string State { get; set; }
}
/*
public abstract class BaseProviderStateMiddleware(RequestDelegate next)
{
    protected abstract IDictionary<string, Action> ProviderStates { get; }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.Value != "/provider-states")
        {
            await next(context);
            return;
        }

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid request method or empty body.");
            return;
        }

        string jsonRequestBody;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8))
        {
            jsonRequestBody = await reader.ReadToEndAsync();
        }

        try
        {
            var providerState = JsonConvert.DeserializeObject<ProviderState>(jsonRequestBody);

            if (!string.IsNullOrEmpty(providerState?.State) &&
                ProviderStates.TryGetValue(providerState.State, out var action))
            {
                action();
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(string.Empty);
            }
            else
            {
                Console.WriteLine($"Unknown provider state: '{providerState?.State}'");
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync($"Unknown provider state: '{providerState?.State}'");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Provider state deserialization failed: {ex}");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error in provider state handler.");
        }
    }
}


public class ProviderState
{
    [JsonProperty("state")]
    public string State { get; set; }

    [JsonProperty("consumer")]
    public string Consumer { get; set; }

    [JsonProperty("params")]
    public Dictionary<string, object> Params { get; set; }

    [JsonProperty("action")]
    public string Action { get; set; }
}
*/
