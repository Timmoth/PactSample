using System.Text.Json.Serialization;
using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/api/users/{id}", async (int id) =>
    {
        var user = new User(id, "timmoth");
        return Results.Ok(user);
    })
    .WithName("GetShipment");

app.Run();

record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username
);