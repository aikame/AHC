using ADDC.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Net;
using System.Text;
using System.Drawing;
using ADDC.Interfaces;

namespace ADDC.Services
{
    public class ComputerInfoService : IComputerInfoService, IHostedService, IDisposable
    {
        private readonly ILogger<ComputerInfoService> _logger;
        private readonly HttpClient _client;
        private readonly IPowershellSessionPoolService _sessionPool;
        private Timer _timer;
        string _coreAddress;
        public ComputerInfoService(IConfiguration configuration,IPowershellSessionPoolService sessionPool, ILogger<ComputerInfoService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _client = httpClientFactory.CreateClient("computerinfoservice");
            _sessionPool = sessionPool;
            _coreAddress = configuration["core"];
            
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
            var res = CollectAndSendInfo();
        }
        public async Task<JObject> CollectInfo()
        {
            var final = await _sessionPool.ExecuteFunction("CollectInfo");
            return JObject.Parse(final);
        }
        public async Task<bool> CollectAndSendInfo()
        {
            JObject jsonData = CollectInfo().Result;
            Console.WriteLine($"GetComputerinfo: {jsonData.ToString()}");

            var serData = JsonConvert.SerializeObject(jsonData);

            var jsonContent = new StringContent(serData, Encoding.UTF8, "application/json");
            var result =  await _client.PostAsync("https://" + _coreAddress + "/CollectComputerInfo", jsonContent);
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
