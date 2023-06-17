using Frontend.Models;
using Nest;

namespace Frontend.Extension
{
    public static class ElasticSearchExtension
    {
        public static void AddElasticSearch(this IServiceCollection services, IConfiguration configuration)
        {
            var baseUrl = configuration["ElasticSettings:baseUrl"];
            var index = configuration["ElasticSettings:defaultIndex"];
            var settings = new ConnectionSettings(new Uri(baseUrl ?? ""))
                .PrettyJson()
                .CertificateFingerprint("9856397ae122494a001a8961b394a3d53e14d18ebb1931b4893c122032deec78")
                .BasicAuthentication("elastic", "a19d36Ga1c0*K43=STEA")
                .DefaultIndex(index); // Сертификат и пользователя придется выставлять в ручную ┐(￣ヘ￣)┌
            settings.EnableApiVersioningHeader();
            var client = new ElasticClient(settings);
            services.AddSingleton<IElasticClient>(client);
            CreateIndex(client, index);
        }
        public static void AddDefaultMappings(ConnectionSettings settings)
        {
            // тут чето добавить можно, пока хз
        }
        public static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName,
                c => c.Map(m => m
                .AutoMap<AppointmentModel>()
                .AutoMap<CitiesModel>()
                .AutoMap<CompaniesModel>()
                .AutoMap<DepartmentModel>())
                );
        }
    }
}
