
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Backend.Models;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System.Web;
using System.Text.Json;
using System.Xml.Linq;
using System;
using System.Text.Json.Nodes;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Net.NetworkInformation;
using Backend.Services;
using Microsoft.Extensions.Logging;
using System.DirectoryServices.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Json;

namespace Backend.Controllers
{

    [ApiController]
    [Route("/old")]
    public class UserController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<UserController> _logger;
        public UserController(ILogger<UserController> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("сlient");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
        }
        private bool CheckPing(string address)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var reply= ping.Send(address);

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

        [HttpPost("CreateProfile")]
        public async Task<IActionResult> ProfileCreation([FromBody] ProfileModel user, [FromQuery] string? domain, [FromQuery] bool blocked = false)
        {
            Console.WriteLine($"ProfileCreation: {JsonConvert.SerializeObject(user)}");

            var profileResponse = await _client.PostAsync("https://localhost:7080/profile/add",
                new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

            if (!profileResponse.IsSuccessStatusCode)
            {
                _logger.LogError("Error: " + profileResponse);
                return BadRequest("Ошибка при создании профиля.");
            }

            string responseContent = await profileResponse.Content.ReadAsStringAsync();
            var jsonProfile = JObject.Parse(responseContent);

            if (!user.ADreq)
                return Content(jsonProfile["id"]?.ToString());

            _ = Task.Run(async () =>
            {
                try
                {
                    var searchComputerResponse = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                    if (!searchComputerResponse.IsSuccessStatusCode) return;

                    var computer = JObject.Parse(await searchComputerResponse.Content.ReadAsStringAsync());

                    var adResponse = await _client.PostAsync($"https://{computer["ipAddress"]}:{_connectorPort}/UserCreation",
                        new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

                    if (!adResponse.IsSuccessStatusCode) return;

                    var jsonADData = JObject.Parse(await adResponse.Content.ReadAsStringAsync());
                    jsonADData["ProfileId"] = jsonProfile["id"];
                    jsonADData["Domain"] = new JObject { ["Forest"] = domain };

                    var mailProfile = new JObject { ["name"] = jsonADData["SamAccountName"] };
                    var emailResponse = await _client.PostAsync($"https://{computer["ipAddress"]}:{_connectorPort}/CreateMailBox",
                        new StringContent(JsonConvert.SerializeObject(mailProfile), Encoding.UTF8, "application/json"));

                    if (emailResponse.IsSuccessStatusCode)
                    {
                        var jsonMail = JObject.Parse(await emailResponse.Content.ReadAsStringAsync());
                        jsonProfile["email"] = jsonMail["Address"];
                    }

                    var temp = await _client.PostAsync("https://localhost:7080/profile/add-adaccount",
                        new StringContent(JsonConvert.SerializeObject(jsonADData), Encoding.UTF8, "application/json"));
                    
                    _logger.LogInformation(temp.ToString());

                    await _client.PostAsync("https://localhost:7080/profile/update",
                        new StringContent(JsonConvert.SerializeObject(jsonProfile), Encoding.UTF8, "application/json"));
                    if (blocked) {
                        JObject sdata = new JObject();
                        sdata["name"] = jsonADData["SamAccountName"].ToString();
                        var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                        await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/BanUser", jsonContent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("[CreateProfile async part]: " + ex.Message);
                }
            });

            return Content(jsonProfile["id"]?.ToString());
        }
        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo([FromQuery] string id, [FromQuery] string domain)
        {
            //'http://127.0.0.2:8000/api/GetComputer?domain={domain}'
            Console.WriteLine(domain + "\\" + id);
            JObject sdata = new JObject();
            sdata["login"] = id;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["login"].ToString());
            var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
            string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
            JObject computer = JObject.Parse(searchComputer);
            var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
            var result = await _client.GetAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/GetInfo?samAccountName=" + id);
            var responseContent = await result.Content.ReadAsStringAsync();
            Console.WriteLine(responseContent);
            if (result.IsSuccessStatusCode)
            {
                var jsonADData = JObject.Parse(responseContent);
                jsonADData["Domain"] = new JObject
                {
                    ["Forest"] = domain
                };
                var responseUpdateAd = _client.PostAsync($"https://localhost:7080/profile/update-adaccount", new StringContent(jsonADData.ToString(), Encoding.UTF8, "application/json"));
                return Content(responseContent);
            }
            else
            {
                return BadRequest("Произошла ошибка при выполнении запроса.");
            }
            
        }
        [HttpGet("BanUser")]
        public async Task<IActionResult> BanUser([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject sdata = new JObject();
            sdata["SamAccountName"] = decodedId;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["name"].ToString());

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/BanUser", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            
        }
        [HttpGet("UnbanUser")]
        public async Task<IActionResult> UnbanUser([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject sdata = new JObject();
            sdata["SamAccountName"] = decodedId;
            Console.WriteLine($"UnbanUser: {sdata}");
            Console.WriteLine(sdata["name"].ToString());

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/UnbanUser", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            
        }
        [HttpPost("AddToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/AddToGroup", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
        }

        [HttpPost("CollectComputerInfo")]
        public async Task<IActionResult> CollectComputerInfo([FromBody] JsonElement data)
        {
            Console.WriteLine(data);
            var domainName = data.GetProperty("DomainName");
            Console.WriteLine(domainName);
            ComputerModel computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(data.ToString());

                var result = await _client.PostAsync("https://localhost:7080/computer/add", new StringContent(System.Text.Json.JsonSerializer.Serialize(data),
                                 Encoding.UTF8, "application/json"));

                var jsonContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                //var result = await _client.PostAsync("https://" + _connectorAddress + "/AddToGroup", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
        }
        [HttpGet("CheckComputer")]
        public async Task<IActionResult> CheckComputer([FromQuery] string _id)
        {
            // https://localhost:7080/api/GetComputer?ComputerName=DC-1


                var result = await _client.GetAsync($"https://localhost:7080/search/computer?query={_id}");
                string responseContent = await result.Content.ReadAsStringAsync();
                ComputerModel computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(responseContent);
                computer.Status = CheckPing(computer.IPAddress);
                var updResult = await _client.PostAsync("https://localhost:7080/computer/add", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
                        Encoding.UTF8, "application/json"));
                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"{computer.ComputerName} ({computer.IPAddress}) changed state to: {computer.Status}");
                    return Ok();
                }
                else
                {
                    _logger.LogError($"Error changing state {computer.ComputerName} ({computer.IPAddress}) to: {computer.Status}");
                    return BadRequest();
                }
            
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);


            var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
            string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
            JObject computer = JObject.Parse(searchComputer);
            var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
            var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/ChangePassword", jsonContent);
            Console.WriteLine(result.ToString());
            if (result.IsSuccessStatusCode)
            {
                JObject response = new JObject();
                response["password"] = await result.Content.ReadAsStringAsync();
                _logger.LogInformation("AHTUNG!!!: " + response.ToString());
                return Ok(JsonConvert.SerializeObject(response));
            }
            else
            {
                return BadRequest();
            }
            
        }
        [HttpGet("CreateMailBox")]
        public async Task<IActionResult> CreateMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject jsonData = new JObject();
            jsonData["name"] = decodedId;
            Console.WriteLine($"Prepared: {jsonData}");
            Console.WriteLine(jsonData["name"].ToString());

            var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
            string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
            JObject computer = JObject.Parse(searchComputer);
            var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
            var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/CreateMailBox", jsonContent);

            var stringResult = await result.Content.ReadAsStringAsync();
            
            JObject jsonMail = new JObject();
            if (result.IsSuccessStatusCode)
            {
                jsonMail = JObject.Parse(stringResult);
            }
            _ = Task.Run(async () =>
            {
                try
                {
                    JObject jsonMail = new JObject();

                    if (result.IsSuccessStatusCode)
                    {
                        var responseMail = await result.Content.ReadAsStringAsync();
                        Console.WriteLine(responseMail);
                        jsonMail = JObject.Parse(responseMail);
                    }
                    

                    var responseSearchUser = await _client.GetAsync($"https://localhost:7080/search/oneprofile?query={id}");
                    if (!responseSearchUser.IsSuccessStatusCode) return;

                    var searchUserJson = JObject.Parse(await responseSearchUser.Content.ReadAsStringAsync());


                    searchUserJson["email"] = jsonMail["Address"];

                    Console.WriteLine($"profile update: {searchUserJson}");

                    await _client.PostAsync("https://localhost:7080/profile/update",
                        new StringContent(JsonConvert.SerializeObject(searchUserJson), Encoding.UTF8, "application/json"));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[UserCreation background] {ex.Message}");
                }
            });

            Console.WriteLine(result.ToString());
            if (result.IsSuccessStatusCode)
            {
                return Ok("Запрос выполнен успешно.");
            }
            else
            {
                return BadRequest("Произошла ошибка при выполнении запроса.");
            }
            
        }
        [HttpGet("HideMailBox")]
        public async Task<IActionResult> HideMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject jsonData = new JObject();
            jsonData["name"] = decodedId;
            Console.WriteLine($"Prepared: {jsonData}");
            Console.WriteLine(jsonData["name"].ToString());

            /*JObject jsonData = JObject.Parse(id);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            Console.WriteLine(user.Name);*/

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/HideMailBox", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok("Запрос выполнен успешно.");
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            
        }

        [HttpGet("GetAppInfo")]
        public async Task<IActionResult> GetAppInfo([FromQuery] string computer, [FromQuery] string domain)
        {
            Console.WriteLine(computer + " get apps");


                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/onecomputer?query={computer}");
        
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computerObj = JObject.Parse(searchComputer);
                var result = await _client.GetAsync("https://" + computerObj["ipAddress"].ToString() + ":" + _connectorPort + "/GetAppInfo");
                Console.WriteLine(result.ToString());
                string responseContent = await result.Content.ReadAsStringAsync();
                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            
        }
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);


                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/CreateGroup", jsonContent);

                string responseContent = await result.Content.ReadAsStringAsync();
                var groupAD = JObject.Parse(responseContent);
                groupAD["Domain"] = new JObject
                {
                    ["Forest"] = jsonData["domain"]
                };
                var databaseReq = await _client.PostAsync("https://localhost:7080/group/add",
                    new StringContent(groupAD.ToString(), Encoding.UTF8, "application/json"));
                Console.WriteLine(result.ToString());
                Console.WriteLine(databaseReq.ToString());
                if (result.IsSuccessStatusCode && databaseReq.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
        }

        [HttpGet("GetGroupMembers")]
        public async Task<IActionResult> GetGroupMembers([FromQuery] string group, [FromQuery] string domain)
        {

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var result = await _client.GetAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/GetGroupMembers?group=" + group);
                string responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                Console.WriteLine(result);
                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            
        }
        [HttpPost("RemoveFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);


                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/RemoveFromGroup", jsonContent);
                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
        }
        [HttpGet("ShowMailBox")]
        public async Task<IActionResult> ShowMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject jsonData = new JObject();
            jsonData["name"] = decodedId;
            Console.WriteLine($"Prepared: {jsonData}");
            Console.WriteLine(jsonData["name"].ToString());

                var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/ShowMailBox", jsonContent);

                Console.WriteLine(result.ToString());
                if (result.IsSuccessStatusCode)
                {
                    return Ok("Запрос выполнен успешно.");
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> UserCreation([FromBody] JsonElement user, [FromQuery] string domain, [FromQuery] bool mail)
        {
            Console.WriteLine(user);

            var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
            if (!responseSearchComputer.IsSuccessStatusCode)
                return BadRequest("Не удалось найти контроллер домена.");

            var searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
            var computer = JObject.Parse(searchComputer);

            var adResponse = await _client.PostAsync($"https://{computer["ipAddress"]}:{_connectorPort}/UserCreation",
                new StringContent(System.Text.Json.JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"));

            var adContent = await adResponse.Content.ReadAsStringAsync();
            Console.WriteLine(adContent);

            if (!adResponse.IsSuccessStatusCode)
                return BadRequest("Ошибка при создании AD пользователя.");

            _ = Task.Run(async () =>
            {
                try
                {
                    var jsonADData = JObject.Parse(adContent);
                    JObject jsonMail = new JObject();

                    if (mail)
                    {
                        var mailProfile = new JObject
                        {
                            ["name"] = jsonADData["SamAccountName"]
                        };

                        var resultEmail = await _client.PostAsync(
                            $"https://{computer["ipAddress"]}:{_connectorPort}/CreateMailBox",
                            new StringContent(JsonConvert.SerializeObject(mailProfile), Encoding.UTF8, "application/json")
                        );

                        if (resultEmail.IsSuccessStatusCode)
                        {
                            var responseMail = await resultEmail.Content.ReadAsStringAsync();
                            Console.WriteLine(responseMail);
                            jsonMail = JObject.Parse(responseMail);
                        }
                    }

                    var responseSearchUser = await _client.GetAsync($"https://localhost:7080/search/oneprofile?query={user.GetProperty("id")}");
                    if (!responseSearchUser.IsSuccessStatusCode) return;

                    var searchUserJson = JObject.Parse(await responseSearchUser.Content.ReadAsStringAsync());

                    jsonADData["Domain"] = new JObject { ["Forest"] = domain };
                    jsonADData["ProfileId"] = searchUserJson["id"];

                    await _client.PostAsync("https://localhost:7080/profile/add-adaccount",
                        new StringContent(JsonConvert.SerializeObject(jsonADData), Encoding.UTF8, "application/json"));

                    if (mail)
                        searchUserJson["email"] = jsonMail["Address"];
                    else if (searchUserJson["Email"] == null)
                        searchUserJson["email"] = "";

                    Console.WriteLine($"profile update: {searchUserJson}");

                    await _client.PostAsync("https://localhost:7080/profile/update",
                        new StringContent(JsonConvert.SerializeObject(searchUserJson), Encoding.UTF8, "application/json"));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[UserCreation background] {ex.Message}");
                }
            });

            return Content(adContent);
        }


        [HttpPost("FireUser")]
        public async Task<IActionResult> FireUser([FromBody] JsonElement data)
        {
            //var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(data);
            var id = new JsonObject
            {
                ["id"] = data.GetProperty("id").GetString()
            };
            Console.WriteLine(id.ToJsonString());
            //JsonElement fire_date = data.GetProperty("fire_date");


                //var jsonContent = new StringContent(id.ToJsonString(), Encoding.UTF8, "application/json");
                var result = await _client.GetAsync($"https://localhost:7080/search/oneprofile?query={id}");
                string responseContent = await result.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseContent);
                JsonElement root = document.RootElement;

                //JsonElement hits = root.GetProperty("hits").GetProperty("hits");

                JObject userJSON = JObject.Parse(responseContent);
                JsonElement profiles = root.GetProperty("profiles");

                foreach (JsonElement profile in profiles.EnumerateArray())
                {
                    if (profile.TryGetProperty("AD", out JsonElement ad))
                    {
                        var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={ad.GetProperty("domain")}");
                        string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                        JObject computer = JObject.Parse(searchComputer);
                        string domain = ad.GetProperty("domain").GetString();
                        string user = ad.GetProperty("samAccountName").GetString();
                        var sdata = new JsonObject
                        {
                            ["name"] = user
                        };


                        var jsonUserBanContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                        var banResult = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/BanUser", jsonUserBanContent);
                        Console.WriteLine($"Domain: {domain}, User: {user}");
                    }
                }
                
                var fireDate = DateTime.UtcNow;
                userJSON["fireDate"] = fireDate;
                var resultUpdProfile = await _client.PostAsync("https://localhost:7080/profile/update", new StringContent(JsonConvert.SerializeObject(userJSON),
                         Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode && resultUpdProfile.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
            return Ok();
        }
        [HttpPost("ReturnUser")]
        public async Task<IActionResult> ReturnUser([FromBody] JsonElement data)
        {
            Console.WriteLine(data);

                var result = await _client.GetAsync($"https://localhost:7080/search/oneprofile?query={data.GetProperty("id")}");
                string responseContent = await result.Content.ReadAsStringAsync();
                var userJSON = JObject.Parse(responseContent);
                Console.WriteLine($"ReturnUser date: {userJSON}");
                userJSON["fireDate"] = null;
                var resultUpdProfile = await _client.PostAsync("https://localhost:7080/profile/update", new StringContent(JsonConvert.SerializeObject(userJSON),
                         Encoding.UTF8, "application/json"));

                if (resultUpdProfile.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            
        }

        [HttpGet("domainList")]
        public async Task<IActionResult> GetDomainList([FromQuery] string? data)
        {
                var responseComputers = await _client.GetAsync($"https://localhost:7080/search/domain");
                string domainsRes = await responseComputers.Content.ReadAsStringAsync();
                List<JObject> JSONdomains = Newtonsoft.Json.JsonConvert.DeserializeObject<List<JObject>>(domainsRes);
                List<string> domains = new List<string>();
                foreach (JObject domainJSON in JSONdomains)
                {
                    string domain = domainJSON["forest"].ToString();
                    domains.Add(domain);
                }

                if (domains.Count > 0)
                {
                    string jsonDomains = System.Text.Json.JsonSerializer.Serialize<List<string>>(domains);
                    Console.Write(jsonDomains);
                    return Content(jsonDomains);
                }
                else
                {
                    Console.Write("No domains");
                    return BadRequest();
                }
        }

        [HttpPost("Authentication")]
        public async Task<IActionResult> Authentication([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);



            try
            {

                string user = data.GetProperty("user").GetString();
                string password = data.GetProperty("password").GetString();
                string[] userParts = user.Split('\\');
                string username = userParts.Length > 1 ? userParts[1] : userParts[0];
                string domain = userParts.Length > 1 ? userParts[0] : "";
                var newJson = new JsonObject
                {
                    ["user"] = username,
                    ["password"] = password
                };

                    var responseSearchComputer = await _client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                    string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                    JObject computer = JObject.Parse(searchComputer);
                    var jsonContent = new StringContent(newJson.ToString(), Encoding.UTF8, "application/json");
                    var result = await _client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/Authentication", jsonContent);
                    Console.WriteLine(result.ToString());
                    if (result.IsSuccessStatusCode)
                    {
                        return Ok();
                    }
                    else
                    {
                        return BadRequest(result);
                    }
                

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
