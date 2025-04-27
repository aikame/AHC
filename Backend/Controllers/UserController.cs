
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Backend.models;
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

namespace Backend.Controllers
{

    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly ILogger<ComputerStateService> _logger;
        public UserController(ILogger<ComputerStateService> logger, IConfiguration configuration)
        {
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
        public async Task<IActionResult> ProfileCreation([FromBody] ProfileModel user, [FromQuery] string? domain)
        {
            Console.WriteLine($"ProfileCreation: {JsonConvert.SerializeObject(user)}");

            using (HttpClient client = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(10.0) })
            {
                var profileTask = client.PostAsync("https://localhost:7080/profile/add",
                    new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

                // ищем DC
                Task<HttpResponseMessage>? searchComputerTask = null;
                if (user.ADreq)
                {
                    searchComputerTask = client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                }

                var profileResponse = await profileTask;

                if (!profileResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Error: " + profileResponse);
                    return BadRequest("Ошибка при создании профиля.");
                }
                string responseContent = await profileResponse.Content.ReadAsStringAsync();
                var jsonProfile = JObject.Parse(responseContent);// JsonConvert.DeserializeObject<JObject>(JsonConvert.DeserializeObject<string>(responseContent));
                Console.WriteLine($"Parsed JSON Response: {jsonProfile}");

                // Если AD не нужен, сразу возвращаем ID
                if (!user.ADreq)
                {
                    return Content(jsonProfile["id"].ToString());
                }

                if (searchComputerTask == null) return BadRequest("Ошибка при поиске компьютера.");
                var searchComputerResponse = await searchComputerTask;
                if (!searchComputerResponse.IsSuccessStatusCode) return BadRequest("Ошибка при получении компьютера.");

                JObject computer = JObject.Parse(await searchComputerResponse.Content.ReadAsStringAsync());

                // создание AD-учетки
                var adTask = client.PostAsync($"https://{computer["ipAddress"]}:{_connectorPort}/UserCreation",
                    new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json"));

                var adResponse = await adTask;
                if (!adResponse.IsSuccessStatusCode) return BadRequest("Ошибка при создании AD профиля.");

                JObject jsonADData = JObject.Parse(await adResponse.Content.ReadAsStringAsync());
                jsonADData["ProfileModelId"] = jsonProfile["id"];
                //создание почтового ящика
                var mailProfile = new JObject { ["name"] = jsonADData["SamAccountName"] };
                var emailTask = client.PostAsync($"https://{computer["ipAddress"]}:{_connectorPort}/CreateMailBox",
                    new StringContent(JsonConvert.SerializeObject(mailProfile), Encoding.UTF8, "application/json"));


                var emailResponse = await emailTask;
                if (emailResponse.IsSuccessStatusCode)
                {
                    JObject jsonMail = JObject.Parse(await emailResponse.Content.ReadAsStringAsync());
                    jsonProfile["email"] = jsonMail["Address"];
                }
                // обновляем профиль

                jsonADData["Domain"] = domain;
                jsonADData["ProfileModelId"] = jsonProfile["id"];
                var resultaddad = await client.PostAsync("https://localhost:7080/profile/add-adaccount",
                    new StringContent(JsonConvert.SerializeObject(jsonADData), Encoding.UTF8, "application/json"));
                var resultUpdProfile = await client.PostAsync("https://localhost:7080/profile/update",
                    new StringContent(JsonConvert.SerializeObject(jsonProfile), Encoding.UTF8, "application/json"));

                return resultUpdProfile.IsSuccessStatusCode ? Content(jsonProfile["id"].ToString()) : BadRequest("Ошибка обновления профиля.");
            }
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
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/GetInfo", jsonContent);
                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            }
        }
        [HttpGet("BanUser")]
        public async Task<IActionResult> BanUser([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject sdata = new JObject();
            sdata["name"] = decodedId;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["name"].ToString());
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/BanUser", jsonContent);
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
        }
        [HttpGet("UnbanUser")]
        public async Task<IActionResult> UnbanUser([FromQuery] string id, [FromQuery] string domain)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject sdata = new JObject();
            sdata["name"] = decodedId;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["name"].ToString());
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/UnbanUser", jsonContent);
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
        }
        [HttpPost("AddToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/AddToGroup", jsonContent);
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
        }

        [HttpPost("CollectComputerInfo")]
        public async Task<IActionResult> CollectComputerInfo([FromBody] JsonElement data)
        {
            Console.WriteLine(data);
            var domainName = data.GetProperty("DomainName");
            Console.WriteLine(domainName);
            ComputerModel computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(data.ToString());
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var result = await client.PostAsync("https://localhost:7080/computer/add", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
                                 Encoding.UTF8, "application/json"));

                var jsonContent = new StringContent(data.ToString(), Encoding.UTF8, "application/json");
                //var result = await client.PostAsync("https://" + _connectorAddress + "/AddToGroup", jsonContent);
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
        }
        [HttpGet("CheckComputer")]
        public async Task<IActionResult> CheckComputer([FromQuery] string _id)
        {
            // https://localhost:7080/api/GetComputer?ComputerName=DC-1
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {

                var result = await client.GetAsync($"https://localhost:7080/search/computer?query={_id}");
                string responseContent = await result.Content.ReadAsStringAsync();
                ComputerModel computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(responseContent);
                computer.Status = CheckPing(computer.IPAddress);
                var updResult = await client.PostAsync("https://localhost:7080/computer/add", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
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
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/ChangePassword", jsonContent);
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
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/CreateMailBox", jsonContent);

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
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/HideMailBox", jsonContent);
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
        }

        [HttpGet("GetAppInfo")]
        public async Task<IActionResult> GetAppInfo([FromQuery] string computer, [FromQuery] string domain)
        {
            Console.WriteLine(computer + " get apps");

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/onecomputer?query={computer}");
        
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computerObj = JObject.Parse(searchComputer);
                var result = await client.GetAsync("https://" + computerObj["ipAddress"].ToString() + ":" + _connectorPort + "/GetAppInfo");
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
        }
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/CreateGroup", jsonContent);

                string responseContent = await result.Content.ReadAsStringAsync();
                var databaseReq = await client.PostAsync("https://localhost:7080/group/add",
                    new StringContent(responseContent, Encoding.UTF8, "application/json"));
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
        }

        [HttpGet("GetGroupMembers")]
        public async Task<IActionResult> GetGroupMembers([FromQuery] string group, [FromQuery] string domain)
        {
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var result = await client.GetAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/GetGroupMembers?group=" + group);
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
        }
        [HttpPost("RemoveFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={data.GetProperty("domain")}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/RemoveFromGroup", jsonContent);
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
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/ShowMailBox", jsonContent);

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
        }
        [HttpPost("CreateUser")]
        public async Task<IActionResult> UserCreation([FromBody] JsonElement user, [FromQuery] string domain, [FromQuery] bool mail)
        {
            Console.WriteLine(user);

            using (HttpClient client = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(5.0) })
            {
                var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                JObject computer = JObject.Parse(searchComputer);
                Console.WriteLine($"CreateUser: {user}");
                var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/UserCreation", new StringContent(System.Text.Json.JsonSerializer.Serialize(user),
                                  Encoding.UTF8, "application/json"));

                string responseADContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine($"{responseADContent}");
                if (result.IsSuccessStatusCode)
                {
                    JObject jsonADData = JObject.Parse(responseADContent);
                    JObject jsonMail = new JObject();
                    if (mail)
                    {

                        JObject mailProfile = new JObject();
                        mailProfile["name"] = jsonADData["SamAccountName"];
                        var resultEmail = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/CreateMailBox", new StringContent(JsonConvert.SerializeObject(mailProfile),
                            Encoding.UTF8, "application/json"));
                        if (resultEmail.IsSuccessStatusCode)
                        {
                            string responseMail = await resultEmail.Content.ReadAsStringAsync();
                            Console.WriteLine(responseMail);
                            jsonMail = JObject.Parse(responseMail);
                        }
                    }



                    string distinguishedName = jsonADData["DistinguishedName"].ToString();
                    string[] parts = distinguishedName.Split(',');

                    var responseSearchUser = await client.GetAsync($"https://localhost:7080/search/oneprofile?query={user.GetProperty("id").ToString()}");
                    string SearchUser = await responseSearchUser.Content.ReadAsStringAsync();
                    JObject SearchUserJson = JObject.Parse(SearchUser);
                    jsonADData["Domain"] = domain;
                    jsonADData["ProfileModelId"] = SearchUserJson["id"];
                    var resultaddad = await client.PostAsync("https://localhost:7080/profile/add-adaccount",
                        new StringContent(JsonConvert.SerializeObject(jsonADData), Encoding.UTF8, "application/json"));

                   

                    if (mail)
                    {
                        SearchUserJson["email"] = jsonMail["Address"];
                    }
                    else if (SearchUserJson["Email"] == null)
                    {
                        SearchUserJson["email"] = "";
                    }

                    Console.WriteLine($"profile update: {SearchUserJson}");
                    var resultupdd = await client.PostAsync("https://localhost:7080/profile/update",
                        new StringContent(JsonConvert.SerializeObject(SearchUserJson), Encoding.UTF8, "application/json"));

          
                    if (resultupdd.IsSuccessStatusCode)
                    {
                        return Content(responseADContent);
                    }
                    else
                    {
                        return BadRequest(resultupdd);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
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
            using (HttpClient client = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(10.0) })
            {

                //var jsonContent = new StringContent(id.ToJsonString(), Encoding.UTF8, "application/json");
                var result = await client.GetAsync($"https://localhost:7080/search/oneprofile?query={id}");
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
                        var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={ad.GetProperty("domain")}");
                        string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                        JObject computer = JObject.Parse(searchComputer);
                        string domain = ad.GetProperty("domain").GetString();
                        string user = ad.GetProperty("samAccountName").GetString();
                        var sdata = new JsonObject
                        {
                            ["name"] = user
                        };


                        var jsonUserBanContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                        var banResult = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/BanUser", jsonUserBanContent);
                        Console.WriteLine($"Domain: {domain}, User: {user}");
                    }
                }
                
                var fireDate = DateTime.UtcNow;
                userJSON["fireDate"] = fireDate;
                var resultUpdProfile = await client.PostAsync("https://localhost:7080/profile/update", new StringContent(JsonConvert.SerializeObject(userJSON),
                         Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode && resultUpdProfile.IsSuccessStatusCode)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            return Ok();
        }
        [HttpPost("ReturnUser")]
        public async Task<IActionResult> ReturnUser([FromBody] JsonElement data)
        {
            Console.WriteLine(data);
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var result = await client.GetAsync($"https://localhost:7080/search/oneprofile?query={data.GetProperty("id")}");
                string responseContent = await result.Content.ReadAsStringAsync();
                var userJSON = JObject.Parse(responseContent);
                Console.WriteLine($"ReturnUser date: {userJSON}");
                userJSON["fireDate"] = null;
                var resultUpdProfile = await client.PostAsync("https://localhost:7080/profile/update", new StringContent(JsonConvert.SerializeObject(userJSON),
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
        }

        [HttpGet("domainList")]
        public async Task<IActionResult> GetDomainList([FromQuery] string? data)
        {

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var responseComputers = await client.GetAsync($"https://localhost:7080/search/computer");
                string computers = await responseComputers.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(computers);
                JsonElement root = document.RootElement;
                List<string> domains = new List<string>();

                foreach (JsonElement hit in root.EnumerateArray())
                {
                    string domain = hit.GetProperty("domainName").ToString();
                    if (!domains.Contains(domain))
                    {
                        domains.Add(domain);
                    }
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
                using (HttpClient client = new HttpClient(new HttpClientHandler()) { Timeout = TimeSpan.FromMinutes(10.0) })
                {
                    var responseSearchComputer = await client.GetAsync($"https://localhost:7080/search/domain-controller?domain={domain}");
                    string searchComputer = await responseSearchComputer.Content.ReadAsStringAsync();
                    JObject computer = JObject.Parse(searchComputer);
                    var jsonContent = new StringContent(newJson.ToString(), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("https://" + computer["ipAddress"].ToString() + ":" + _connectorPort + "/Authentication", jsonContent);
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

            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

    }
}
