using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


var ElasticSettings = new ElasticsearchClientSettings(new Uri("https://localhost:9200"))
    .CertificateFingerprint("9856397ae122494a001a8961b394a3d53e14d18ebb1931b4893c122032deec78")
    .Authentication(new BasicAuthentication("elastic", "a19d36Ga1c0*K43=STEA"));
var ElasticClient = new ElasticsearchClient(ElasticSettings);