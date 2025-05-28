using Backend.Interfaces;
using Backend.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.Services
{
    public class AccountService : IAccountService
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<AccountService> _logger;
        private readonly IComputerService _computerService;
        private readonly IProfileService _profileService;
        public AccountService(ILogger<AccountService> logger, IHttpClientFactory httpClientFactory, IComputerService computerService, IProfileService profileService, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("AccountService");
            _logger = logger;
            _computerService = computerService;
            _profileService = profileService;
            _connectorPort = configuration["connectorPort"];
        }

        public async Task<ADAccountModel?> Create(ProfileModel profile, DomainModel domain)
        {
            try
            {
                ComputerModel computer = await _computerService.FindDCinDomain(domain);
                if (computer is null) { return null; }

                var adResponse = await _client.PostAsync($"https://{computer.IPAddress}:{_connectorPort}/UserCreation",
                    new StringContent(JsonConvert.SerializeObject(profile), Encoding.UTF8, "application/json"));
                if (!adResponse.IsSuccessStatusCode)
                    return null;

                var acc = JsonConvert.DeserializeObject<ADAccountModel>(await adResponse.Content.ReadAsStringAsync());
                acc.ProfileId = profile.Id.ToString();
                acc.Domain = domain;

                var updateDB = await _client.PostAsync("https://localhost:7080/profile/add-adaccount",
                        new StringContent(JsonConvert.SerializeObject(acc), Encoding.UTF8, "application/json"));

                return updateDB.IsSuccessStatusCode ? acc : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[Create]: " + e.Message);
                return null;
            }
        }
        public async Task<ADAccountModel?> Get(ADAccountModel user)
        {
            try
            {
                ComputerModel computer = await _computerService.FindDCinDomain(user.Domain);
                if (computer is null) { return null; }

                var result = await _client.GetAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/GetInfo?samAccountName=" + user.SamAccountName);
                if (!result.IsSuccessStatusCode) { 
                    return null;
                }

                string stringAdaccount = await result.Content.ReadAsStringAsync();
                ADAccountModel? aDAccount = JsonConvert.DeserializeObject<ADAccountModel>(stringAdaccount);
                aDAccount.Domain = user.Domain;
                return aDAccount;
            }
            catch (Exception e) { 
                _logger.LogError("[Get]: " + e.Message);
                return null;
            }

        }
        public async Task<bool> Ban(ADAccountModel account)
        {
            try
            {

                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }
                var jsonContent = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" +computer.IPAddress + ":" + _connectorPort + "/BanUser", jsonContent);
                
                return result.IsSuccessStatusCode; 
            }
            catch (Exception e)
            {
                _logger.LogError($"[Ban]: {e.Message}");
                return false;
            }
        }

        public async Task<bool> Unban(ADAccountModel account)
        {
            try
            {

                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }

                var jsonContent = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/UnbanUser", jsonContent);

                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError($"[Ban]: {e.Message}");
                return false;
            }
        }

        public async Task<string?> ChangePassword(ADAccountModel data)
        {
            try
            {
                ComputerModel? computer = await _computerService.FindDCinDomain(data.Domain);
                if (computer is null) { return null; }

                var jsonContent = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/ChangePassword", jsonContent);

                return result.IsSuccessStatusCode ? await result.Content.ReadAsStringAsync() : null;
            }
            catch (Exception e)
            {
                _logger.LogError($"[ChangePassword]: {e.Message}");
                return null;
            }

        }

        public async Task<bool> CreateMailBox(ADAccountModel account)
        {
            try
            {
                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }

                var jsonContent = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/CreateMailBox", jsonContent);

                if (!result.IsSuccessStatusCode)
                    return false;

                JObject jsonMail = JObject.Parse(await result.Content.ReadAsStringAsync());

                ProfileModel? profile = await _profileService.Get(account.SamAccountName);
                if (profile is null) { return false; }

                profile.Email = jsonMail["Address"].ToString();
                var isUpdated = await _profileService.Update(profile);

                return isUpdated;
            }
            catch (Exception e)
            {
                _logger.LogError($"[CreateMailBox]: {e.Message}");
                return false;
            }
        }

        public async Task<bool> HideMailBox(ADAccountModel account)
        {
            try
            {

                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }

                var jsonContent = new StringContent(JObject.FromObject(account).ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/HideMailBox", jsonContent);

                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError($"[HideMailBox]: {e.Message}");
                return false;
            }
        }

        public async Task<bool> ShowMailBox(ADAccountModel account)
        {
            try
            {

                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }

                var jsonContent = new StringContent(JObject.FromObject(account).ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer.IPAddress + ":" + _connectorPort + "/ShowMailBox", jsonContent);

                return result.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogError($"[ShowMailBox]: {e.Message}");
                return false;
            }
        }


        public async Task<bool> Authentication(ADAccountModel account, string password)
        {
            try
            {
                ComputerModel? computer = await _computerService.FindDCinDomain(account.Domain);
                if (computer is null) { return false; }

                var newJson = new JObject
                {
                    ["user"] = account.SamAccountName,
                    ["password"] = password
                };
                var jsonContent = new StringContent(newJson.ToString(), Encoding.UTF8, "application/json");

                var result = await _client.PostAsync("https://" + computer.IPAddress+ ":" + _connectorPort + "/Authentication", jsonContent);

                return result.IsSuccessStatusCode;
            }
            catch (Exception e) {
                _logger.LogError($"[Authentication]: {e.Message}");
                return false;
            }
        }
    }


}
