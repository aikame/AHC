
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Backend.models;
using System.Text;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
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
                return true; // !!! Не используйте в Production
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
                    var resultAD = await client.PostAsync("https://192.168.64.164:7093/UserCreation", new StringContent(JsonConvert.SerializeObject(user),
                        Encoding.UTF8, "application/json"));

                    string responseADContent = await resultAD.Content.ReadAsStringAsync();
                    Console.WriteLine(responseADContent);
                    if (resultAD.IsSuccessStatusCode)
                    {
                        
                        JObject jsonData = JObject.Parse(responseADContent);
                        JObject profileData = new JObject();
                        JObject profileAdData = new JObject();
                        profileData["_id"] = jsonProfile["_id"];
                        profileAdData["ObjectGUID"] = jsonData["ObjectGUID"];
                        profileAdData["DistinguishedName"] = jsonData["DistinguishedName"];
                        profileData["profile"] = profileAdData;
                        var resultUpdProfile = await client.PostAsync("http://127.0.0.2:8000/api/add_to_profiles", new StringContent(JsonConvert.SerializeObject(profileData),
                                 Encoding.UTF8, "application/json"));
                        Console.WriteLine(resultUpdProfile);
                        if (resultUpdProfile.IsSuccessStatusCode)
                        {
                            return Ok(resultUpdProfile);
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
        [HttpPost("GetInfo")]
        public async Task<IActionResult> GetInfo([FromBody] UserInfoRequest data)
        {
            Console.WriteLine(data.User);
            Console.WriteLine(data.Domain);
            string log = data.User;
            //JObject jsonData = JObject.Parse(data);
            //string user = jsonData["user"].ToString();
            //string domain = jsonData["domain"].ToString();
            JObject sdata = new JObject();
            sdata["login"] = data.User.ToString();
            Console.WriteLine($"Prepared: {sdata}");
            Console.WriteLine(sdata["login"].ToString());
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata.ToString(), Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://" + data.Domain + "/GetInfo", jsonContent);

                //var result = await client.PostAsJsonAsync("https://"+data.Domain + "/GetInfo", sdata);
                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);
                //UserModel usr = JsonConvert.DeserializeObject<UserModel>(responseContent);
                //Console.WriteLine(usr.Name);
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
        [HttpPost("BanUser")]
        public async Task<IActionResult> BanUser([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/BanUser", JsonConvert.SerializeObject(user));
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
        [HttpPost("UnbanUser")]
        public async Task<IActionResult> UnbanUser([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/UnbanUser", JsonConvert.SerializeObject(user));
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
        [HttpPost("AddToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            string group = jsonData["group"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            var requestData = new
            {
                user = user,
                group = group
            };
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/AddToGroup", JsonConvert.SerializeObject(requestData));
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
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            string password = jsonData["password"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            var requestData = new
            {
                user = user,
                password = password
            };
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/ChangePassword", JsonConvert.SerializeObject(requestData));
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
        [HttpPost("CreateMailBox")]
        public async Task<IActionResult> CreateMailBox([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/CreateMailBox", JsonConvert.SerializeObject(user));
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
        [HttpPost("HideMailBox")]
        public async Task<IActionResult> HideMailBox([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/HideMailBox", JsonConvert.SerializeObject(user));
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
        public async Task<IActionResult> RemoveFromGroup([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            string group = jsonData["group"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            var requestData = new
            {
                user = user,
                group = group
            };
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/RemoveFromGroup", JsonConvert.SerializeObject(requestData));
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
        [HttpPost("ShowMailBox")]
        public async Task<IActionResult> ShowMailBox([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();
            Console.WriteLine(user.Name);
            Console.WriteLine(domain);
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/ShowMailBox", JsonConvert.SerializeObject(user));
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


    }
}
