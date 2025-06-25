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

        if (string.IsNullOrEmpty(pactBrokerUrl) || string.IsNullOrEmpty(pactBrokerToken))
        {
            throw new InvalidOperationException("Pact Broker URL or Token not set in environment variables.");
        }

        pactVerifier
            .WithHttpEndpoint(_testServer.ServerUri)
            .WithPactBrokerSource(new Uri(pactBrokerUrl), options =>
            {
                options.TokenAuthentication(pactBrokerToken);
                
                var sha = Environment.GetEnvironmentVariable("GITHUB_SHA");
                var branch = Environment.GetEnvironmentVariable("GITHUB_REF_NAME");
                if (string.IsNullOrWhiteSpace(sha) || string.IsNullOrWhiteSpace(branch))
                {
                    return;
                }

                // https://docs.pact.io/pact_broker/pacticipant_version_numbers
                var version = $"{sha[..7]}-{branch}";

                options.PublishResults(version,
                    results =>
                    {
                        // https://github.com/pact-foundation/pact-net/issues/376
                        results.BuildUri(_testServer.ServerUri);
                        results.ProviderTags("master");
                    });
                
            })
            .WithProviderStateUrl(new Uri(_testServer.ServerUri, "/provider-states"))
            .Verify();

    }
}
