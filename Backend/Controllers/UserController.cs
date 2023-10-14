using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;
using Backend;

namespace Backend.Controllers
{
    public class UserController
    {
        public static async void GetInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = File.ReadAllText("./PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddArgument(context.Request.Query["UserLogin"]).Invoke();
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
        }
        public static async void UserCreation(HttpContext context, UserModel user)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = File.ReadAllText("../../../PowershellFunctions/UserCreation.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("name", user.name);
                parameters.Add("surname", user.surname);
                parameters.Add("midname", user.midname);
                parameters.Add("city", user.city);
                parameters.Add("company", user.company);
                parameters.Add("department", user.department);
                parameters.Add("appointment", user.appointment);

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
