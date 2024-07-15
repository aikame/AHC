
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

namespace Backend.Controllers
{
    public class CustomHttpClientHandler : HttpClientHandler
    {
        public CustomHttpClientHandler()
        {
            ServerCertificateCustomValidationCallback = ValidateServerCertificate;
        }

        private bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors)
        {
            if (errors == SslPolicyErrors.None)
            {
                return true;
            }

            // Для разработки, игнорируем ошибки сертификата
            return true; // !!! Не используйте в Production (как же похуй)
        }
    }
    
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        private readonly string _connectorAddress;
        private readonly ILogger<ComputerStateService> _logger;
        public UserController(ILogger<ComputerStateService> logger,IConfiguration configuration)
        {
            _logger = logger;
            _connectorAddress = configuration["ConnectorAddress"];
        }
        private bool CheckPing(string address)
        {
            using (var ping = new Ping())
            {
                try
                {
                    var reply = ping.Send("address");

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
        public async Task<IActionResult> ProfileCreation([FromBody] ProfileModel user)
        {

            Console.WriteLine(user.name);
            //Console.WriteLine(domain);

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                Console.WriteLine($"ProfileCreation: {JsonConvert.SerializeObject(user)}");
                var result = await client.PostAsync("http://127.0.0.2:8000/api/put", new StringContent(JsonConvert.SerializeObject(user),
                                 Encoding.UTF8, "application/json"));
                string responseContent = await result.Content.ReadAsStringAsync();

                var unescapedContent = JsonConvert.DeserializeObject<string>(responseContent);

                // Преобразование ответа в JSON объект
                var jsonProfile = JsonConvert.DeserializeObject<JObject>(unescapedContent);
                Console.WriteLine("Parsed JSON Response: " + jsonProfile.ToString());

                Console.WriteLine(responseContent);
                if (result.IsSuccessStatusCode)
                {   
                    if (!user.ADreq)
                    {
                        Console.WriteLine(jsonProfile["_id"]);
                        return Content(jsonProfile["_id"].ToString());
                    }
                    var resultAD = await client.PostAsync("https://"+ _connectorAddress + "/UserCreation", new StringContent(JsonConvert.SerializeObject(user),
                        Encoding.UTF8, "application/json"));
                    string responseADContent = await resultAD.Content.ReadAsStringAsync();
                    Console.WriteLine(responseADContent);
                    if (resultAD.IsSuccessStatusCode)
                    {
                        JObject jsonADData = JObject.Parse(responseADContent);
                        JObject mailProfile = new JObject();
                        mailProfile["name"] = jsonADData["SamAccountName"];
                        var resultEmail = await client.PostAsync("https://" + _connectorAddress + "/CreateMailBox", new StringContent(JsonConvert.SerializeObject(mailProfile),
                            Encoding.UTF8, "application/json"));
                        JObject jsonMail =new JObject();
                        if (resultEmail.IsSuccessStatusCode)
                        {
                            string responseMail = await resultEmail.Content.ReadAsStringAsync();
                            Console.WriteLine(responseMail);
                            jsonMail = JObject.Parse(responseMail);
                        }


                        string distinguishedName = jsonADData["DistinguishedName"].ToString();
                        string[] parts = distinguishedName.Split(',');

                        string domain = parts[2].Split('=')[1];

                        // Создание нового JSON-объекта
                        var newJson = new JObject(
                            new JProperty("AD",
                                new JObject(
                                    new JProperty("domain", domain),
                                    new JProperty("user", jsonADData["SamAccountName"])
                                )
                            )
                        );

                        // Преобразование нового JSON-объекта в строку
                        string newJsonString = newJson.ToString(Formatting.Indented);
                        Console.WriteLine(newJsonString);


                        JObject profileData = new JObject();
                        profileData["_id"] = jsonProfile["_id"];
                        profileData["profile"] = newJson;
                        profileData["email"] = jsonMail["Address"];
                        Console.WriteLine($"profile update: {profileData}");
                        var resultUpdProfile = await client.PostAsync("http://127.0.0.2:8000/api/add_to_profiles", new StringContent(JsonConvert.SerializeObject(profileData),
                                 Encoding.UTF8, "application/json"));
                        Console.WriteLine(resultUpdProfile);
                        if (resultUpdProfile.IsSuccessStatusCode)
                        {
                            return Content(jsonProfile["_id"].ToString());
                        }
                        else
                        {
                            return BadRequest(resultUpdProfile);
                        }
                    }
                    else
                    {
                        return BadRequest("Произошла ошибка при создании AD профиля.");
                    }

                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            }
        }
        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo([FromQuery] string id)
        {
            Console.WriteLine(id);
            JObject sdata = new JObject();
            sdata["login"] = id;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["login"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/GetInfo", jsonContent);
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
        public async Task<IActionResult> BanUser([FromQuery] string id)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id +" -> "+ decodedId);
            JObject sdata = new JObject();
            sdata["name"] = decodedId;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["name"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/BanUser", jsonContent);
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
        public async Task<IActionResult> UnbanUser([FromQuery] string id)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject sdata = new JObject();
            sdata["name"] = decodedId;
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["name"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/UnbanUser", jsonContent);
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

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://"+ _connectorAddress+ "/AddToGroup", jsonContent);
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
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var result = await client.PostAsync("http://127.0.0.2:8000/api/ComputerData", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
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
            // http://127.0.0.2:8000/api/GetComputer?ComputerName=DC-1
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var result = await client.GetAsync($"http://127.0.0.2:8000/api/GetComputer?_id={_id}");
                string responseContent = await result.Content.ReadAsStringAsync();
                ComputerModel computer = System.Text.Json.JsonSerializer.Deserialize<ComputerModel>(responseContent);
                computer.Status = CheckPing(computer.IPAddress);
                var updResult = await client.PostAsync("http://127.0.0.2:8000/api/ComputerData", new StringContent(System.Text.Json.JsonSerializer.Serialize(computer),
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

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/ChangePassword", jsonContent);
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
        public async Task<IActionResult> CreateMailBox([FromQuery] string id)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject jsonData = new JObject();
            jsonData["name"] = decodedId;
            Console.WriteLine($"Prepared: {jsonData}");
            Console.WriteLine(jsonData["name"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/CreateMailBox", jsonContent);

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
        public async Task<IActionResult> HideMailBox([FromQuery] string id)
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
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/HideMailBox", jsonContent);
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
        [HttpPost("RemoveFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] JsonElement data)
        {
            var temp = System.Text.Json.JsonSerializer.Serialize(data);
            Console.WriteLine(temp);
            JObject jsonData = JObject.Parse(temp);

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress+ "/RemoveFromGroup", jsonContent);
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
        public async Task<IActionResult> ShowMailBox([FromQuery] string id)
        {
            string decodedId = HttpUtility.HtmlDecode(id);
            Console.WriteLine(id + " -> " + decodedId);
            JObject jsonData = new JObject();
            jsonData["name"] = decodedId;
            Console.WriteLine($"Prepared: {jsonData}");
            Console.WriteLine(jsonData["name"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + _connectorAddress + "/ShowMailBox", jsonContent);

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
        public async Task<IActionResult> UserCreation([FromBody] UserModel user)
        {
            //JObject jsonData = JObject.Parse(data);
            //UserModel user = jsonData["user"].ToObject<UserModel>();
            //string domain = jsonData["domain"].ToString();

            Console.WriteLine(user.Name);
            //Console.WriteLine(domain);

            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                Console.WriteLine($"Getinfo: {JsonConvert.SerializeObject(user)}");
                var result = await client.PostAsync("/UserCreation", new StringContent(JsonConvert.SerializeObject(user),
                                  Encoding.UTF8, "application/json"));
                //var result = await client.PostAsJsonAsync(domain+ "/UserCreation", JsonConvert.SerializeObject(user));
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
            JsonElement fire_date = data.GetProperty("fire_date");
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

                var jsonContent = new StringContent(id.ToJsonString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("http://127.0.0.2:8000/api/getone", jsonContent);
                string responseContent = await result.Content.ReadAsStringAsync();
                JsonDocument document = JsonDocument.Parse(responseContent);
                JsonElement root = document.RootElement;

                JsonElement hits = root.GetProperty("hits").GetProperty("hits");

                foreach (JsonElement hit in hits.EnumerateArray())
                {
                    JsonElement source = hit.GetProperty("_source");
                    JsonElement profiles = source.GetProperty("profiles");

                    foreach (JsonElement profile in profiles.EnumerateArray())
                    {
                        if (profile.TryGetProperty("AD", out JsonElement ad))
                        {
                            // Выполнение необходимого действия для каждого объекта "AD"
                            string domain = ad.GetProperty("domain").GetString();
                            string user = ad.GetProperty("user").GetString();
                            var sdata = new JsonObject
                            {
                                ["name"] = user
                            };


                            var jsonUserBanContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                            var banResult = await client.PostAsync("https://" + _connectorAddress + "/BanUser", jsonUserBanContent);
                            // Пример действия: вывод на консоль
                            Console.WriteLine($"Domain: {domain}, User: {user}");
                        }
                    }
                }
                Console.WriteLine($"fire date: {fire_date}");
                var resultUpdProfile = await client.PostAsync("http://127.0.0.2:8000/api/fire_user", new StringContent(System.Text.Json.JsonSerializer.Serialize(data),
                         Encoding.UTF8, "application/json"));

                if (result.IsSuccessStatusCode)
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
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {

               
                Console.WriteLine($"fire date: {data}");
                var resultUpdProfile = await client.PostAsync("http://127.0.0.2:8000/api/return_user", new StringContent(System.Text.Json.JsonSerializer.Serialize(data),
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
                using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
                {

                    var jsonContent = new StringContent(newJson.ToString(), Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("https://" + _connectorAddress + "/Authentication", jsonContent);
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
