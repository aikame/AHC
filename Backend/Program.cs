using Backend;
using Backend.Services;
public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        host.Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseUrls("https://0.0.0.0:7095/");
                webBuilder.UseStartup<Backend.Startup>();
            });
            /*.ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                services.AddHostedService<ComputerStateService>();
            });*/
}
