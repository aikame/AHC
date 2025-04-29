using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.Text;
using Backend.Controllers;
using Backend.models;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.Extensions.Logging;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.DirectoryServices.ActiveDirectory;
namespace Backend.Services
{
    public class ComputerStateService : IHostedService, IDisposable
    {
        private readonly ILogger<ComputerStateService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private Timer _timer;
        public ComputerStateService(ILogger<ComputerStateService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("ComputerStateService is starting.");

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            CheckComputerState();
        }

        private async void CheckComputerState()
        {

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                try
                {
                    var result = await client.GetAsync("https://localhost:7080/search/computer");
                    string responseContent = await result.Content.ReadAsStringAsync();
                    List<JObject> root = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JObject>>(responseContent);
                    //JsonDocument document = JsonDocument.Parse(responseContent);
                    //JsonElement root = document.RootElement;

                    if (root.Count == 0)
                    {
                        _logger.LogError("No computers at database");
                        return;
                    }

                    foreach (JObject item in root)
                    {
                        string address = item["ipAddress"].ToString();
                        bool status = CheckPing(address);

                        item["status"] = status;
                        item["Domain"] = new JObject
                        {
                            ["Forest"] = item["domainName"]
                        };
                        var content = new StringContent(item.ToString(), Encoding.UTF8, "application/json");
    
                        var updResult = await client.PostAsync("https://localhost:7080/computer/update", content);

                        if (updResult.IsSuccessStatusCode)
                        {
                            _logger.LogInformation($"{item["computerName"]} ({address}) changed state to: {status}");
                        }
                        else
                        {
                            _logger.LogError($"Error changing state {item["computerName"]} ({address}) to: {status}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("failed to check ComputerState or database in unavailable");
                }
                
            }

        }
        private bool CheckPing(string address)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var reply = ping.Send(address);

                    if (reply.Status == IPStatus.Success)
                    {
                        _logger.LogInformation("Ping to {address} successful: {RoundtripTime}ms",address, reply.RoundtripTime);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Ping to {address} failed: {Status}", address, reply.Status);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Ping to {address} failed: {Exception}", address, ex.Message);
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
