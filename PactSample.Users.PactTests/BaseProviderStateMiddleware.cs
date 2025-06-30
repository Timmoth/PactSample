using System.Net;
using System.Text;
using System.Text.Json;

namespace PactSample.Users.PactTests;
public class BaseProviderStateMiddleware(RequestDelegate next, 
    ILogger<BaseProviderStateMiddleware> logger, 
    ProviderStates providerStates,
    IServiceProvider serviceProvider)
{
    public async Task Invoke(HttpContext context)
    {
        logger.LogDebug("➡️ Invoking provider states middleware");

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
        logger.LogDebug("📩 Handling provider-state request at: {Path}", context.Request.Path);

        try
        {
            if (context.Request.Method.Equals(HttpMethod.Post.Method, StringComparison.OrdinalIgnoreCase)
                && context.Request.Body != null)
            {
                using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
                var jsonRequestBody = await reader.ReadToEndAsync();

                logger.LogDebug("📦 Received provider state JSON body: {Json}", jsonRequestBody);

                var providerState = JsonSerializer.Deserialize<ProviderState>(
                    jsonRequestBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                if (providerState == null)
                {
                    logger.LogWarning("⚠️ Failed to deserialize provider state.");
                    return;
                }

                if (!string.IsNullOrEmpty(providerState.State)
                    && providerStates.States.TryGetValue(providerState.State, out var setupAction))
                {
                    logger.LogInformation("✅ Running provider state setup: {State}", providerState.State);
                    setupAction?.Invoke(serviceProvider);
                }
                else
                {
                    logger.LogWarning("⚠️ Unknown provider state: '{State}'", providerState.State);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Exception while handling provider state.");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }

        logger.LogDebug("✅ Provider state middleware completed successfully.");
    }
}

public class ProviderState
{
    public string Consumer { get; set; }
    public string State { get; set; }
}