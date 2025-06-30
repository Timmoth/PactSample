using Microsoft.AspNetCore.Http;
using PactSample.Users.PactTests;

public class TestStateProvider : BaseProviderStateMiddleware
{
    public TestStateProvider(RequestDelegate next) : base(next)
    {
    }

    protected override IDictionary<string, Action> ProviderStates { get; }
}