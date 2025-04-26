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
        private readonly PowershellSessionPoolService _sessionPool;
        private Timer _timer;
        string _coreAddress;
        //InitialSessionState iss;
        public ComputerInfoService(PowershellSessionPoolService sessionPool, ILogger<ComputerInfoService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _sessionPool = sessionPool;
            string jsonText = System.IO.File.ReadAllText("config.txt");

            JObject config = JObject.Parse(jsonText);
            _coreAddress = config["core"].ToString();
            
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("PostRequestService is starting.");

            //SendPostRequest();
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(30));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            var res = SendPostRequest();
        }

        private async Task<bool> SendPostRequest()
        {

            var func = _sessionPool.ExecuteFunction("CollectInfo");
            string final = func.Result;

            Console.WriteLine($"GetComputerinfo: {final}");
            JObject jsonData = JObject.Parse(final);


            var serData = JsonConvert.SerializeObject(jsonData);

            using (HttpClient nclient = new HttpClient(new HttpClientHandler()))
            {
                var jsonContent = new StringContent(serData, Encoding.UTF8, "application/json");
                var result =  await nclient.PostAsync("https://" + _coreAddress + "/CollectComputerInfo", jsonContent);
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
