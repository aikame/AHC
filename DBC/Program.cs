using DBC.Data;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
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
                webBuilder.UseStartup<DBC.Startup>();
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.AddHttpClient();
                var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
    .DefaultIndex("profiles");

                var elasticClient = new ElasticsearchClient(settings);
                services.AddSingleton<ElasticsearchClient>(elasticClient);
                var configuration = hostContext.Configuration;
                services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("Postgres")));
            });
}
