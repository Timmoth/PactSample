using Microsoft.AspNetCore.Http;
using PactSample.Users.PactTests;

public class TestStateProvider : BaseProviderStateMiddleware
{
    public TestStateProvider(RequestDelegate next) : base(next)
    {
    }
    
    protected override IDictionary<string, Action> ProviderStates => new Dictionary<string, Action>
    {
        ["A user with ID 4 exists"] = () =>
        {
            Console.WriteLine("Setting up provider state: A user with ID 4 exists");
            // Optional setup logic
        }
    };
}