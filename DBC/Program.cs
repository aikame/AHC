using DBC.Data;
using DBC.Interfaces;
using DBC.Services;
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

                services.AddHostedService<ElasticIndexStateService>();
                services.AddScoped<ISearchService, SearchService>();
                services.AddScoped<IAccountService, AccountService>();
                services.AddScoped<IComputerService, ComputerService>();
                services.AddScoped<IGroupService, GroupService>();
                services.AddScoped<IProfileService, ProfileService>();
            });
}
