using System.Text.Json.Serialization;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<UsersStartup>();
    });    
host.Build().Run();
