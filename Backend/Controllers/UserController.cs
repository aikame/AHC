using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;

namespace Backend.Controllers
{
    public class UserController
    {
        public static async void GetUserInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("./PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {

                    final += result.ToString();
                }
                await context.Response.WriteAsync(final);
            }
        }
        public static async void GetGroupInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("./PowershellFunctions/GetGroupInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("GroupLogin", context.Request.Query["GroupLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final != "404")
                {
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(final);
                }
                else
                {
                    context.Response.StatusCode = Int32.Parse(final);
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void BanUser(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("./PowershellFunctions/BanUser.ps1");
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

                string scriptText = File.ReadAllText("./PowershellFunctions/UnbanUser.ps1");
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
                string scriptText = File.ReadAllText("./PowershellFunctions/AddToGroup.ps1");

                var results = ps.AddScript(scriptText)
                    .AddParameter("GroupLogin", context.Request.Query["GroupLogin"]).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
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
                    Console.WriteLine($"GroupLogin = {context.Request.Query["GroupLogin"]}" +
                        $"UserLogin = {context.Request.Query["UserLogin"]}");
                    await context.Response.WriteAsync("Произошла ошибка");
                }
            }
        }
        public static async void RemoveFromGroup(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("./PowershellFunctions/RemoveFromGroup.ps1");
                var results = ps.AddScript(scriptText)
                    .AddParameter("GroupLogin", context.Request.Query["GroupLogin"]).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
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
                string scriptText = File.ReadAllText("./PowershellFunctions/ChangePassw.ps1");
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

                string scriptText = File.ReadAllText("./PowershellFunctions/CreateMailBox.ps1");
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

                string scriptText = File.ReadAllText("./PowershellFunctions/HideMailBox.ps1");
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
        
        public static async void ShowMailBox(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("./PowershellFunctions/ShowMailBox.ps1");
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
        public static async void UserCreation(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("./PowershellFunctions/UserCreation.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("name", context.Request.Query["name"]);
                parameters.Add("surname", context.Request.Query["surname"]);
                parameters.Add("midname", context.Request.Query["midname"]);
                parameters.Add("city", context.Request.Query["city"]);
                parameters.Add("company", context.Request.Query["company"]);
                parameters.Add("department", context.Request.Query["department"]);

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
        

    }
}
