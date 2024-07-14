using ADDC.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Net;
using System.Text;

namespace ADDC.Services
{
    public class ComputerInfoService : IHostedService, IDisposable
    {
        private readonly ILogger<ComputerInfoService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private Timer _timer;
        string _coreAddress;
        PowerShell ps;
        InitialSessionState iss;
        public ComputerInfoService(ILogger<ComputerInfoService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            string jsonText = System.IO.File.ReadAllText("config.txt");

            JObject config = JObject.Parse(jsonText);
            _coreAddress = config["core"].ToString();
            ps = PowerShell.Create();
            iss = InitialSessionState.CreateDefault();
            string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/CollectInfo.ps1");
            ps.AddScript(scriptText);
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PostRequestService is starting.");

            SendPostRequest();
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var res = SendPostRequest();
        }

        private async Task<bool> SendPostRequest()
        {
            string final = "";

            
            var results = ps.Invoke();
                
            foreach (var errorRecord in ps.Streams.Error)
            {
                Console.WriteLine("Error: " + errorRecord.Exception.Message);
            }
            foreach (var scriptResult in results)
            {
                final += scriptResult.ToString();
            }
            
            Console.WriteLine($"GetComputerinfo: {final}");
            JObject jsonData = JObject.Parse(final);


            var serData = JsonConvert.SerializeObject(jsonData);

            using (HttpClient nclient = new HttpClient(new CustomHttpClientHandler()))
            {
                var jsonContent = new StringContent(serData, Encoding.UTF8, "application/json");
                var result = await nclient.PostAsync("https://" + _coreAddress + "/CollectComputerInfo", jsonContent);
                Console.WriteLine(result);
                if (result.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Data transferred successfully to {_coreAddress}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to transfer data to {_coreAddress}");
                    Console.WriteLine(result);
                    return false;
                }
             }
            
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PostRequestService is stopping.");

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
