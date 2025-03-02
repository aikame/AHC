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

            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            CheckComputerState();
        }

        private async void CheckComputerState()
        {

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var result = await client.GetAsync("http://127.0.0.2:8000/api/ComputerData");
                string responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                JsonDocument document = JsonDocument.Parse(responseContent);
                JsonElement root = document.RootElement;
                JsonElement hitstemp, hits;
                if(!root.TryGetProperty("hits",out hitstemp))
                {
                    _logger.LogError($"No computers at database");
                    return;
                }
                if (!hitstemp.TryGetProperty("hits", out hits))
                {
                    _logger.LogError($"No computers at database");
                    return;
                }
                //JsonElement hits = root.GetProperty("hits").GetProperty("hits");

                foreach (JsonElement hit in hits.EnumerateArray())
                {
                    JsonElement source = hit.GetProperty("_source");
                    string address = source.GetProperty("IPAddress").ToString();
                    var computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(source);
                    computer.Status = CheckPing(address);

                    var updResult = await client.PostAsync("http://127.0.0.2:8000/api/ComputerData", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
                        Encoding.UTF8, "application/json"));
                    if (result.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"{computer.ComputerName} ({computer.IPAddress}) changed state to: {computer.Status}");
                    }
                    else
                    {
                        _logger.LogError($"Error changing state {computer.ComputerName} ({computer.IPAddress}) to: {computer.Status}");
                    }
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
