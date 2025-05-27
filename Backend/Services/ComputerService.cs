using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;
using System.Text;
using Newtonsoft.Json;
using System.Net.NetworkInformation;

namespace Backend.Services
{
    public class ComputerService : IComputerService
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<ComputerService> _logger;
        public ComputerService(ILogger<ComputerService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("ComputerService");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
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
                        _logger.LogInformation("Ping to {address} successful: {RoundtripTime}ms", address, reply.RoundtripTime);
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
        public async Task<ComputerModel?> FindDCinDomain(DomainModel domain)
        {
            try
            {
                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain.Forest}");
                if (!responseSearchComputer.IsSuccessStatusCode)
                {
                    return null;
                }

                string stringComputer = await responseSearchComputer.Content.ReadAsStringAsync();

                return JObject.Parse(stringComputer).ToObject<ComputerModel>();
            }
            catch (Exception e)
            {
                _logger.LogError("[FindDCinDomain]: " + e.Message);
                return null;
            }

        }
        public async Task<bool> CollectComputerInfo(ComputerModel computer)
        {
            try
            {
                var result = await _client.PostAsync("https://localhost:7080/computer/add", new StringContent(JsonConvert.SerializeObject(computer),
                 Encoding.UTF8, "application/json"));

                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError("[CollectComputerInfo]: "+ e.Message);
                return false;
            }

        }
        public async Task<bool> CheckComputer(ComputerModel computer)
        {
            try
            {
                var result = await _client.GetAsync($"https://localhost:7080/search/computer?query={computer.Id}");
                if (!result.IsSuccessStatusCode)
                    return false;
                computer.Status = CheckPing(computer.IPAddress);

                var updResult = await _client.PostAsync("https://localhost:7080/computer/add", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
                    Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{computer.ComputerName} ({computer.IPAddress}) changed state to: {computer.Status}");
                    return true;
                }
                else
                {
                    _logger.LogError($"Error changing state {computer.ComputerName} ({computer.IPAddress}) to: {computer.Status}");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckComputer]: "+ e.Message);
                return false;
            }

        }

        public async Task<JObject?> GetAppInfo(ComputerModel computer)
        {
            try
            {
                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/onecomputer?query={computer.ComputerName}");
                ComputerModel searchComputer = JsonConvert.DeserializeObject<ComputerModel>(await responseSearchComputer.Content.ReadAsStringAsync());

                var result = await _client.GetAsync("https://" +searchComputer.IPAddress + ":" + _connectorPort + "/GetAppInfo");

                return result.IsSuccessStatusCode ? JObject.Parse(await result.Content.ReadAsStringAsync()) : null;

            }
            catch (Exception e)
            {
                _logger.LogError("[CollectComputerInfo]: " + e.Message);
                return null;
            }

        }

        public async Task<List<String>?> GetDomainList()
        {
            try {
                var responseComputers = await _client.GetAsync($"https://localhost:7080/search/domain");

                List<DomainModel> domains = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DomainModel>>(await responseComputers.Content.ReadAsStringAsync());
                List<string> domainsResult = new List<string>();
                foreach (DomainModel d in domains)
                {
                    string domain = d.Forest;
                    domainsResult.Add(domain);
                }

                return domains.Count > 0 ? domainsResult : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetDomainList]: " + e.Message);
                return null;
            }
        }
    }
}
