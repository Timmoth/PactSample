using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

public class VerifyPactWithConsumer : IClassFixture<ProducerWebApiTestServer>
{
    private readonly ProducerWebApiTestServer _testServer;
    private readonly ITestOutputHelper _output;

    public VerifyPactWithConsumer(ProducerWebApiTestServer testServer, ITestOutputHelper output)
    {
        _testServer = testServer;
        _output = output;
    }

    [Fact]
    public void VerifyPact()
    {
        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Debug,
            Outputters = new List<IOutput>()
            {
                new XunitOutput(_output)
            }
        };

        using var pactVerifier = new PactVerifier("UserService", config);

        var pactBrokerUrl = Environment.GetEnvironmentVariable("PACT_BROKER_URL");
        var pactBrokerToken = Environment.GetEnvironmentVariable("PACT_BROKER_TOKEN");
        var providerVersion = Environment.GetEnvironmentVariable("PACT_PROVIDER_VERSION") ?? "local";

        if (string.IsNullOrEmpty(pactBrokerUrl) || string.IsNullOrEmpty(pactBrokerToken))
        {
            throw new InvalidOperationException("Pact Broker URL or Token not set in environment variables.");
        }

        pactVerifier
            .WithHttpEndpoint(_testServer.ServerUri)
            .WithPactBrokerSource(new Uri(pactBrokerUrl), options =>
            {
                options.TokenAuthentication(pactBrokerToken);
                options.PublishResults(providerVersion);
            })
            .WithProviderStateUrl(new Uri(_testServer.ServerUri, "/provider-states"))
            .Verify();

    }
}
