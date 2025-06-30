using PactNet;
using PactNet.Infrastructure.Outputters;
using PactNet.Output.Xunit;
using PactNet.Verifier;
using Xunit.Abstractions;

public class UsersTestServer : BaseProducerTestServer<UsersStartup>
{
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
    }

    protected override void ConfigureTestWebHost(IWebHostBuilder webHostBuilder)
    {
        base.ConfigureTestWebHost(webHostBuilder);
    }
}

public class UsersPactVerificationTests(BaseProducerTestServer<UsersStartup> testServer, ITestOutputHelper output)
    : BaseVerifyPactWithConsumer<UsersTestServer, UsersStartup>(testServer, output)
{
    [Fact]
    public void EnsureApiHonoursPactWithConsumers()
    {
        RunProducerPactVerification();
    }
}


public abstract class BaseVerifyPactWithConsumer<T, U>(
    BaseProducerTestServer<UsersStartup> testServer,
    ITestOutputHelper output)
    : IClassFixture<T>
    where T : BaseProducerTestServer<U>
    where U : IStartup
{
    protected void RunProducerPactVerification()
    {
        var config = new PactVerifierConfig
        {
            LogLevel = PactLogLevel.Debug,
            Outputters = new List<IOutput>()
            {
                new XunitOutput(output)
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
            .WithHttpEndpoint(testServer.ServerUri)
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
                        results.BuildUri(testServer.ServerUri);
                        results.ProviderTags("master");
                    });
                
            })
            .WithProviderStateUrl(new Uri(testServer.ServerUri, "/provider-states"))
            .Verify();

    }
}
