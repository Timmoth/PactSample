using System.Text.Json.Serialization;

public class UsersStartup : IStartup
{
    public UsersStartup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public IServiceProvider ConfigureServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        return services.BuildServiceProvider();
    }

    public void Configure(IApplicationBuilder app)
    {
        var env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

        if (env.IsDevelopment())
        {
            //...
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