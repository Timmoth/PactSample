using System.Text.Json.Serialization;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Register named HttpClient with base URL from config
builder.Services.AddHttpClient("UserService", client =>
{
    var baseUrl = builder.Configuration["UserService:BaseUrl"];
    if (string.IsNullOrWhiteSpace(baseUrl))
        throw new InvalidOperationException("UserService base URL is not configured.");

    client.BaseAddress = new Uri(baseUrl);
});

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/shipments/{id}", async (int id, IHttpClientFactory httpClientFactory) =>
    {
        var userId = 4; // Simulated value
        var client = httpClientFactory.CreateClient("UserService");

        // BaseAddress is already set, just use relative path
        var userResponse = await client.GetFromJsonAsync<User>($"users/{userId}");

        if (userResponse == null)
        {
            return Results.NotFound("User not found");
        }

        var shipment = new Shipment(id, userId, userResponse.Username);
        return Results.Ok(shipment);
    })
    .WithName("GetShipment");

app.Run();

public record Shipment(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("user_id")] int UserId,
    [property: JsonPropertyName("username")] string Username
);

record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username
);

public partial class Program { }
