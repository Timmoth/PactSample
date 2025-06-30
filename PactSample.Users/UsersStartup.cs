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
            endpoints.MapGet("/users/{id:int}", async context =>
            {
                var idStr = context.Request.RouteValues["id"]?.ToString();
                if (int.TryParse(idStr, out int id))
                {
                    if (id == 4)
                    {
                        var user = new User(id, "test-user");
                        await context.Response.WriteAsJsonAsync(user);
                        return;
                    }
                }

                context.Response.StatusCode = 404;
            });
        });
    }
}

record User(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("username")] string Username
);