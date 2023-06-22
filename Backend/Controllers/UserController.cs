using static Backend.JSON.ChangePasswd ;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;

namespace Backend.Controllers
{
    public class ChangePasswdClass
    {
        public string userLogin { get; set; }
        public string password { get; set; }
    }
    public class UserGroupClass
    {
        public string UserLogin { get; set; }
        public string GroupLogin { get; set; }
    }
    public class UserCreationClass
    {
        public string name {get;set;}
        public string surname {get;set;}
        public string midname { get; set; }
        public string city { get; set; }
        public string company { get; set; }
        public string department { get; set; }
        public string appointment { get; set; }
    }
    public class UserController
    {
        public static async void GetUserInfo(HttpContext context)
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
        public static async void GetGroupInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("../../../PowershellFunctions/GetGroupInfo.ps1");
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

                string scriptText = File.ReadAllText("../../../PowershellFunctions/UnbanUser.ps1");
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
                
                var body = await context.Request.ReadFromJsonAsync<UserGroupClass>();
                if (body == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Отправлен неправильный запрос");
                    return;
                }

                var results = ps.AddScript(scriptText).AddArgument(body.UserLogin).AddArgument(body.GroupLogin)
                    .Invoke();
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
                    context.Response.StatusCode = Int32.Parse("500");
                    Console.WriteLine($"GroupLogin = {body.GroupLogin}" +
                        $"UserLogin = {body.UserLogin}");
                    await context.Response.WriteAsync(final);
                }
            }
        }
        public static async void RemoveFromGroup(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/RemoveFromGroup.ps1");
                var body = await context.Request.ReadFromJsonAsync<UserGroupClass>();
                if (body == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Отправлен неправильный запрос");
                    return;
                }

                var results = ps.AddScript(scriptText).AddArgument(body.UserLogin).AddArgument(body.GroupLogin)
                    .Invoke();
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
                    context.Response.StatusCode = Int32.Parse("500");
                    Console.WriteLine($"GroupLogin = {body.GroupLogin}" +
                        $"UserLogin = {body.UserLogin}");
                    await context.Response.WriteAsync(final);
                }
            }
        }
        public static async void ChangePassword(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                var body = await context.Request.ReadFromJsonAsync<ChangePasswdClass>();
                if (body == null)
                {
                    context.Response.StatusCode=400;
                    await context.Response.WriteAsync("Отправлен неправильный запрос");
                    return ;
                }
                string scriptText = File.ReadAllText("../../../PowershellFunctions/ChangePassw.ps1");
                //System.Collections.IDictionary parameters = new Dictionary<string, string>();
                /*
                parameters.Add("userLogin", body.userLogin);
                parameters.Add("newPasswd", body.password);
                */
                var results = ps.AddScript(scriptText).AddArgument(body.userLogin).AddArgument(body.password).Invoke();
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
                    Console.WriteLine(body.userLogin + body.password);
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
        }
        public static async void UserCreation(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/UserCreation.ps1");

                var body = await context.Request.ReadFromJsonAsync<UserCreationClass>();
                if (body == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Отправлен неправильный запрос");
                    return;
                }

                var results = ps.AddScript(scriptText)
                    .AddArgument(body.name)
                    .AddArgument(body.surname)
                    .AddArgument(body.midname)
                    .AddArgument(body.city)
                    .AddArgument(body.company)
                    .AddArgument(body.department)
                    .AddArgument(body.appointment)
                    .Invoke();
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
