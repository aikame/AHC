using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADDC.Services;
using ADDC.Interfaces;
namespace ADDC
{
    public class Startup
    {
        
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddControllers();
            services.AddHttpClient();
            services.AddHostedService<ComputerInfoService>();
            services.AddSingleton<IComputerInfoService, ComputerInfoService>();
            services.AddSingleton<IPowershellSessionPoolService,PowershellSessionPoolService> ();
            services.AddSingleton<IExchangePowershellSessionPoolService, ExchangePowershellSessionPoolService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
         

        }
    }
}