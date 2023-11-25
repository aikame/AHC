using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace ADDC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls("https://localhost:7096/");
                    webBuilder.UseStartup<Startup>();                    
                });
    }
}