using System.Text.Json.Serialization;

public class UsersStartup
{
    private readonly IConfiguration _configuration;

    public UsersStartup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<UserRepo>();
        services.AddEndpointsApiExplorer();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            // Development-specific setup
        }

        app.UseRouting();
        app.UseHttpsRedirection();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/users/{id:int}", async (HttpContext context, UserRepo userRepo) =>
            {
                var idStr = context.Request.RouteValues["id"]?.ToString();
                if (int.TryParse(idStr, out int id))
                {
                    var user = userRepo.GetUser(id);
                    if (user != null)
                    {
                        await context.Response.WriteAsJsonAsync(user);
                        return;
                    }
                }

                context.Response.StatusCode = 404;
            });
        });
    }
}

public class UserRepo
{
    private static readonly Dictionary<int, User> _users = new Dictionary<int, User>();
    
    public User? GetUser(int id)
    {
        return _users.GetValueOrDefault(id);
    }

    public void AddUser(User user)
    {
        _users.Add(user.Id, user);
    }
}

public record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username
);