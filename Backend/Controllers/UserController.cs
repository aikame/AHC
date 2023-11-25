using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Policy;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        /*public static async void GetInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {

                    final += result.ToString();
                }
                await context.Response.WriteAsync(final);
            }
        }
        public static async void BanUser(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/BanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200") {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
                
            }
        }
        public static async void UnbanUser(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/UnbanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void AddToGroup(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/AddToGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();
                parameters.Add("grpID",context.Request.Query["grpID"]);
                parameters.Add("userID", context.Request.Query["userID"]);
                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";

                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void ChangePassword(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/ChangePassw.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();
                
                parameters.Add("userID", context.Request.Query["userID"]);
                parameters.Add("newPasswd", context.Request.Query["newPasswd"]);

                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";

                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void CreateMailBox(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/CreateMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", context.Request.Query["userLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void HideMailBox(HttpContext context)
        {
            using(var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/HideMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", context.Request.Query["userLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void RemoveFromGroup(HttpContext context)
        {
            using(var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/RemoveFromGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("grpLogin", context.Request.Query["grpLogin"]);
                parameters.Add("userLogin", context.Request.Query["userLogin"]);

                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";

                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void ShowMailBox(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/ShowMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", context.Request.Query["userLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync("Успех");
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }*/
        [HttpPost("CreateUser")]
        public async Task<IActionResult> UserCreation([FromBody] string user)
        {
            UserModel user1 = JsonConvert.DeserializeObject<UserModel>(user);
            Console.WriteLine(user1.name);
            string domain = "localhost:7096";

            using (HttpClient client = new HttpClient())
            {
                // Создаем объект содержащий данные для отправки
                

                // Отправляем POST запрос с данными в виде JSON
                HttpResponseMessage response = await client.PostAsJsonAsync(domain+"/CreateUser", user);

                // Проверяем успешность запроса
                if (response.IsSuccessStatusCode)
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
