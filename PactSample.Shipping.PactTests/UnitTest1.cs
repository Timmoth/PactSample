using PactNet;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PactNet.Output.Xunit;
using Xunit.Abstractions;

public class UserApiPactTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly PactConfig _pactConfig;
    public UserApiPactTest(ITestOutputHelper output, WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _pactConfig = new PactConfig
        {
            PactDir = "../../../../pacts/",
            Outputters = new[]
            {
                new XunitOutput(output)
            },
            LogLevel = PactLogLevel.Debug
        };
    }

    [Fact]
    public async Task GetShipment_ReturnsExpectedData_WhenUserServiceIsStubbed()
    {
        var pact = Pact.V4("ShippingService", "UserService", _pactConfig).WithHttpInteractions();
        
        pact
            .UponReceiving("A request for user with ID 4")
            .Given("A user with ID 4")
            .WithRequest(HttpMethod.Get, "/users/4")
            .WillRespond()
            .WithStatus(200)
            .WithJsonBody(new
            {
                id = 4,
                username = "test-user"
            });
        
        await pact.VerifyAsync((async mockServer =>
        {
            // Override UserService:BaseUrl to use Pact mock server
            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((context, config) =>
                {
                    var dict = new Dictionary<string, string>
                    {
                        ["UserService:BaseUrl"] = mockServer.MockServerUri.ToString()
                    };
                    
                    config.AddInMemoryCollection(dict);
                });
            }).CreateClient();

            var response = await client.GetAsync("/api/shipments/123");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Shipment>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.Equal(123, result!.Id);
            Assert.Equal(4, result.UserId);
            Assert.Equal("test-user", result.Username);
        }));
    }
}