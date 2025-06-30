using Microsoft.AspNetCore.Http;
using PactSample.Users.PactTests;

public class TestStateProvider : BaseProviderStateMiddleware
{
    public TestStateProvider(RequestDelegate next) : base(next)
    {
    }
    
    protected override IDictionary<string, Action> ProviderStates => new Dictionary<string, Action>
    {
        ["A request for user with ID 4"] = () =>
        {
            Console.WriteLine("Setting up provider state: A user with ID 4 exists");
            // Optional setup logic
        }
    };
}