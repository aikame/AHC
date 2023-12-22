using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System;
using Frontend.Classes;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Azure;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        [HttpPost("GetInfo")]
        public async Task<IActionResult> GetInfo([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            string user = jsonData["user"].ToString();
            string domain = jsonData["domain"].ToString();
   
            using (HttpClient client = new HttpClient())
            {


                var result = await client.PostAsJsonAsync(domain + "/GetInfo", JsonConvert.SerializeObject(user));
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
        public async Task<IActionResult> UserCreation([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string domain = jsonData["domain"].ToString();

            Console.WriteLine(user.Name);
            Console.WriteLine(domain);

            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine($"Getinfo: {JsonConvert.SerializeObject(user)}");
                var result = await client.PostAsync(domain + "/UserCreation", new StringContent(JsonConvert.SerializeObject(user),
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
