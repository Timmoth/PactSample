using System.Text.Json;
using System.Text.Json.Serialization;

public partial class Program
{
    public static WebApplication CreateApp(string[]? args = null)
    {
        var builder = WebApplication.CreateBuilder(args ?? Array.Empty<string>());
        builder.Services.AddOpenApi();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.ListenLocalhost(9223); // same port as your ServerUri
        });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapPost("/provider-states", async (HttpContext context) =>
        {
            var providerState = await JsonSerializer.DeserializeAsync<ProviderState>(context.Request.Body);

            if (providerState?.State == "User with ID 4 exists")
            {
                // Simulate any setup needed for this state.
                // In minimal API demo, you may not need to do anything yet.
            }

            context.Response.StatusCode = 200;
            await context.Response.CompleteAsync();
        });


        app.MapGet("/users/{id}", (int id) =>
        {
            // this must match what's in the Pact
            if (id == 4)
            {
                var user = new User(id, "test-user"); // match expected value
                return Results.Ok(user);
            }

            return Results.NotFound();
        });

        return app;
    }

    public static void Main(string[] args)
    {
        var app = CreateApp(args);
        app.Run();
    }
}

record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username
);

record ProviderState(string State, IDictionary<string, string>? Params);
