
using System.Management.Automation;
using Microsoft.AspNetCore.Mvc;
using System.Management.Automation.Runspaces;

using System.Management.Automation.Runspaces;
using System.Management.Automation;
using System.DirectoryServices;
using Microsoft.AspNetCore.Http;
using System;
using ADDC.Models;
using System.Net.Sockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ADDC.Models;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Diagnostics;

namespace ADDC.Controllers
{
    [ApiController]
    [Route("/")]
    public class PowershellController : Controller
    {
        [HttpPost("GetInfo")]
        public ActionResult GetInfo([FromBody] JObject data)
        {
            Console.WriteLine($"Getinfo: {data}");
            var userName = data["login"].ToString();
            //var userName = JsonConvert.DeserializeObject<string>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", userName).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {

                    final += result.ToString();
                }
                Console.WriteLine($"Getinfo: {final}");
                JObject jsonData = JObject.Parse(final);
                Console.WriteLine($"Getinfo: {jsonData}");
                string StringPasswordLastSet = jsonData["PasswordLastSet"]?.ToString();
                var PasswordLastSet = DateTime.Parse(StringPasswordLastSet);
                var userModel = new ADUserModel
                {
                    DistinguishedName = jsonData["DistinguishedName"].ToString(),
                    SamAccountName = jsonData["SamAccountName"].ToString(),
                    Enabled = jsonData["Enabled"].ToString() == "True",
                    EmailAddress = jsonData["EmailAddress"].ToString(),
                    PasswordLastSet = PasswordLastSet,
                    PasswordExpired = jsonData["PasswordExpired"].ToString() == "True",
                    MemberOf = jsonData["MemberOf"].ToObject<List<string>>()
            };

                var response = JsonConvert.SerializeObject(userModel);
                return Content(response);
            }
        }
        [HttpPost("BanUser")]
        public ActionResult BanUser([FromBody] JObject data)
        {
            Console.WriteLine($"BanUser: {data}");
            var user = data.ToObject<UserModel>();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/BanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", user.Name).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("UnbanUser")]
        public ActionResult UnbanUser([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/UnbanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", user.Name).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("AddToGroup")]
        public ActionResult AddToGroup([FromBody] JObject data) {
            Console.WriteLine(JsonConvert.SerializeObject(data));
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/AddToGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();
                parameters.Add("grpID", group);
                parameters.Add("userID", user.Name);
                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("ChangePassword")]
        public ActionResult ChangePassword([FromBody] JObject data)
        {
            UserModel user = data["user"].ToObject<UserModel>();
            string password = data["password"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/ChangePassw.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("userID",user.Name);
                parameters.Add("newPasswd", password);

                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("CreateMailBox")]
        public ActionResult CreateMailBox([FromBody] JObject data)
        {
            Console.WriteLine($"CreateMailBox {data}");
            var user = data.ToObject<UserModel>();
   
            var userName = data["name"].ToString();
            Console.WriteLine(userName);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Unrestricted  -File \"./PowershellFunctions/CreateMailBox.ps1\" -userLogin \"{userName}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

           
            using (var process = Process.Start(startInfo))
            {
                var output = process.StandardOutput.ReadToEnd();
                var errors = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(errors))
                {
                    Console.WriteLine("Error: " + errors);
                    return BadRequest(errors);
                }
                Console.WriteLine(output);
                try
                {
                    // Пытаемся распарсить JSON-ответ от PowerShell скрипта
                    var jsonData = JObject.Parse(output);




                    return Content(jsonData.ToString(), "application/json");
                }
                catch (JsonException ex)
                {
                    Console.WriteLine("Error parsing JSON: " + ex.Message);
                    return BadRequest("Error parsing JSON from PowerShell output");
                }
            }
        }

        [HttpPost("HideMailBox")]
        public ActionResult HideMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/HideMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", user.Name).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("ShowMailBox")]
        public ActionResult ShowMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/ShowMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", user.Name).Invoke();
                string final = "";
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("RemoveFromGroup")]
        public ActionResult RemoveFromGroup([FromBody] JObject data)
        {
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/RemoveFromGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("grpLogin", group);
                parameters.Add("userLogin", user.Name);

                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";

                foreach (var result in results)
                {
                    final += result.ToString();
                }
                if (final == "200")
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }

        [HttpPost("UserCreation")]
        public ActionResult UserCreation([FromBody] JObject data)
        {
            Console.WriteLine($"UserCreation: {data}");
            var user = data.ToObject<UserModel>();
            //var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("./PowershellFunctions/UserCreation.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("name", user.Name);
                parameters.Add("surname", user.SurName);
                parameters.Add("midname", user.Patronymic);
                parameters.Add("city", user.City);
                parameters.Add("company", user.Company);
                parameters.Add("department", user.Department);
                parameters.Add("appointment", user.Appointment);

                var results = ps.AddScript(scriptText).AddParameters(parameters).Invoke();
                string final = "";

                foreach (var result in results)
                {
                    final += result.ToString();
                }
                Console.WriteLine($"Result: {final}");
                string error;
                foreach (var errorRecord in ps.Streams.Error)
                {
                    Console.WriteLine("Error: " + errorRecord.Exception.Message);
                }
                if (final == "400")
                {
                    return BadRequest(final);
                }
                else
                {
                    return Ok(final);
                }
            }
        }
    }

}
