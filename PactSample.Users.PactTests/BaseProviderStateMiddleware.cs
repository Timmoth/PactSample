using System.Net;
using System.Text;
using System.Text.Json;

namespace PactSample.Users.PactTests;

public static class Helpers
{
    public static void DebugLog(string message)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "pact-debug");
        Directory.CreateDirectory(dir); // Ensure it exists

        var logPath = Path.Combine(dir, "pact-provider-debug.log");
        File.AppendAllText(logPath, $"[{DateTime.UtcNow:O}] {message}{Environment.NewLine}");
    }
    
}
public abstract class BaseProviderStateMiddleware(RequestDelegate next)
{
    protected abstract IDictionary<string, Action> ProviderStates { get; }

    public async Task Invoke(HttpContext context)
    {
        Helpers.DebugLog("invoke provider states middleware");

        if (context.Request.Path.Value == "/provider-states")
        {
            await HandleProviderStatesRequestAsync(context);
            await context.Response.WriteAsync(string.Empty);
        }
        else
        {
            await next(context);
        }
    }

    private async Task HandleProviderStatesRequestAsync(HttpContext context)
    {
        context.Response.StatusCode = (int)HttpStatusCode.OK;
        Helpers.DebugLog("provider-state middleware: " + context.Request.Path);

        try
        {
            if (context.Request.Method.Equals(HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase)
                && context.Request.Body != null)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var jsonRequestBody = await reader.ReadToEndAsync();

                Helpers.DebugLog("deserialized json request body");
                Helpers.DebugLog(jsonRequestBody);

                var providerState = JsonSerializer.Deserialize<ProviderState>(
                    jsonRequestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (providerState == null)
                {
                    Helpers.DebugLog("⚠️ Failed to deserialize provider state.");
                    return;
                }

                if (!string.IsNullOrEmpty(providerState.State)
                    && ProviderStates.TryGetValue(providerState.State, out var setupAction))
                {
                    Helpers.DebugLog($"✅ Running provider state setup: {providerState.State}");
                    setupAction?.Invoke();
                }
                else
                {
                    Helpers.DebugLog($"⚠️ Unknown provider state: '{providerState.State}'");
                }
            }
        }
        catch (Exception e)
        {
            Helpers.DebugLog("❌ Exception in HandleProviderStatesRequestAsync");
            Helpers.DebugLog(e.ToString());
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        Helpers.DebugLog("✅ middleware returned 200");
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
