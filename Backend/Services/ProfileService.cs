using Newtonsoft.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Text;
using Newtonsoft.Json.Linq;
using Backend.Interfaces;
using Backend.Models.Data;

namespace Backend.Services
{
    public class ProfileService : IProfileService
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<ProfileService> _logger;
        private readonly IComputerService _computerService;
        private readonly IServiceProvider _provider;
        public ProfileService(ILogger<ProfileService> logger,IServiceProvider provider, IHttpClientFactory httpClientFactory, IComputerService computerService, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("ProfileService");
            _logger = logger;
            _provider = provider;
            _computerService = computerService;
            _connectorPort = configuration["connectorPort"];
        }

        public async Task<Guid?> Create(ProfileModel profile)
        {
            try
            {
                var profileResponse = await _client.PostAsync("https://localhost:7080/profile/add",
                    new StringContent(JsonConvert.SerializeObject(profile), Encoding.UTF8, "application/json"));

                if (!profileResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error: " + profileResponse);
                    return null;
                }
                else
                {
                    string responseContent = await profileResponse.Content.ReadAsStringAsync();
                    var jsonProfile = JObject.Parse(responseContent);
                    return jsonProfile["id"] is not null ? (Guid)jsonProfile["id"] : null;
                }
            }
            catch (Exception e) { 
                _logger.LogError("[Create]: " +e.Message);
                return null;
            }

        }

        public async Task<ProfileModel?> Get(string query)
        {
            try
            {
                var responseSearchUser = await _client.GetAsync($"https://localhost:7080/search/oneprofile?query={query}");
                return responseSearchUser.IsSuccessStatusCode ? JsonConvert.DeserializeObject<ProfileModel>(await responseSearchUser.Content.ReadAsStringAsync()) : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[Get]: " + e.Message);
                return null;
            }
        }
        public async Task<bool> Update(ProfileModel profile)
        {
            try
            {
                var response = await _client.PostAsync("https://localhost:7080/profile/update",
                        new StringContent(JsonConvert.SerializeObject(profile), Encoding.UTF8, "application/json"));
                return response.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError("[Update]: " + e.Message);
                return false;
            }
        }

        public async Task<bool> FireUser(ProfileModel profile)
        {
            try
            {
                var _accountService = _provider.GetRequiredService<IAccountService>();
                foreach (var acc in profile.Profiles)
                {
                    if (acc.ContainsKey("AD"))
                    {
                        ADAccountModel accModel = acc["AD"].ToObject<ADAccountModel>();
                        ComputerModel computer = await _computerService.FindDCinDomain(accModel.Domain);
                        if (computer is null) { continue; }

                        var banRes = await _accountService.Ban(accModel);
                    }
                }
                profile.FireDate = DateTime.UtcNow.ToString();

                var updateRes = await Update(profile);

                return updateRes ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError("[FireUser]: " + e.Message);
                return false;
            }
        }

        public async Task<bool> ReturnUser(ProfileModel profile)
        {
            try
            {
                profile.FireDate = null;

                var updRes = await Update(profile);
                return updRes ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError("[ReturnUser]: " + e.Message);
                return false;
            }

        }
    }
}
