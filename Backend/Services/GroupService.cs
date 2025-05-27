using Backend.Interfaces;
using Backend.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Http.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace Backend.Services
{
    public class GroupService : IGroupService
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<GroupService> _logger;
        private readonly IComputerService _computerService;
        public GroupService(ILogger<GroupService> logger, IHttpClientFactory httpClientFactory, IComputerService computerService, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("GroupService");
            _logger = logger;
            _computerService = computerService;
            _connectorPort = configuration["connectorPort"];
        }
        public async Task<bool> AddToGroup(ADAccountModel user, GroupModel group)
        {
            try {
                ComputerModel computer = await _computerService.FindDCinDomain(user.Domain);
                if (computer is null) { return false; }
                var body = new
                {
                    user = user,    
                    group = group    
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await _client.PostAsync($"https://{computer.IPAddress}:{_connectorPort}/AddToGroup", content);
                return result.IsSuccessStatusCode;
            }
            catch(Exception e) { 
                _logger.LogError("[AddToGroup]: " + e.Message);
                return false;
            }
        }

        public async Task<bool> RemoveFromGroup(ADAccountModel user, GroupModel group)
        {
            try
            {
                ComputerModel computer = await _computerService.FindDCinDomain(user.Domain);
                if (computer is null) { return false; }
                var body = new
                {
                    user = user,
                    group = group
                };

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var result = await _client.PostAsync($"https://{computer.IPAddress}:{_connectorPort}/RemoveFromGroup", content);
                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError("[RemoveFromGroup]: " + e.Message);
                return false;
            }
        }

        public async Task<GroupModel?> CreateGroup(GroupModel group)
        {
            try
            {
                ComputerModel computer = await _computerService.FindDCinDomain(group.Domain);
                if (computer is null) { return null; }

                var jsonContent = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/CreateGroup", jsonContent);

                var createdGroup = JsonConvert.DeserializeObject<GroupModel>(await result.Content.ReadAsStringAsync());

                createdGroup.Domain = group.Domain;

                var databaseReq = await _client.PostAsync("https://localhost:7080/group/add",
                    new StringContent(JsonConvert.SerializeObject(createdGroup), Encoding.UTF8, "application/json"));

                return result.IsSuccessStatusCode && databaseReq.IsSuccessStatusCode && createdGroup is not null ? createdGroup : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateGroup]: " + e.Message);
                return null;
            }

        }

        public async Task<JObject?> GetGroupMembers(GroupModel group)
        {
            try
            {
                ComputerModel computer = await _computerService.FindDCinDomain(group.Domain);
                if (computer is null) { return null; }

                var result = await _client.GetAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/GetGroupMembers?group=" + group.Name);

                return result.IsSuccessStatusCode ? JObject.Parse(await result.Content.ReadAsStringAsync()) : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetGroupMembers]: " + e.Message);
                return null;
            }

        }
    }
}
