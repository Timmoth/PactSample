using Microsoft.AspNetCore.Http;
using PactSample.Users.PactTests;

/*public class TestStateProvider : BaseProviderStateMiddleware
{
    private readonly IServiceProvider _provider;
    public TestStateProvider(
        RequestDelegate next, 
        ILogger<BaseProviderStateMiddleware> logger,
        IServiceProvider provider) : base(next, logger)
    {
        _provider = provider;
    }
    
    protected override IDictionary<string, Action> ProviderStates => new Dictionary<string, Action>
    {
        ["A user with ID 4"] = () =>
        {
            _provider.GetRequiredService<UserRepo>().AddUser(new User(4, "test-user"));
        }
    };
}*/