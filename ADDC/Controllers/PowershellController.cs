
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

namespace ADDC.Controllers
{
    [ApiController]
    [Route("/")]
    public class PowershellController : Controller
    {
        [HttpPost("GetInfo")]
        public ActionResult GetInfo([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<string>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", user).Invoke();
                string final = "";
                foreach (var result in results)
                {

                    final += result.ToString();
                }
                var response = JsonConvert.SerializeObject(final);
                return Content(response);
            }
        }
        [HttpPost("BanUser")]
        public ActionResult BanUser([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/BanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", user.name).Invoke();
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

        [HttpPost("UnbanUser")]
        public ActionResult UnbanUser([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/UnbanUser.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", user.name).Invoke();
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

        [HttpPost("AddToGroup")]
        public ActionResult AddToGroup([FromBody] string data) {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string group = jsonData["group"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/AddToGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();
                parameters.Add("grpID", group);
                parameters.Add("userID", user.name);
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

        [HttpPost("ChangePassword")]
        public ActionResult ChangePassword([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string password = jsonData["password"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/ChangePassw.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("userID",user.name);
                parameters.Add("newPasswd", password);

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

        [HttpPost("CreateMailBox")]
        public ActionResult CreateMailBox([FromBody] string  data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/CreateMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", user.name).Invoke();
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

        [HttpPost("HideMailBox")]
        public ActionResult HideMailBox([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/HideMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", user.name).Invoke();
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

        [HttpPost("ShowMailBox")]
        public ActionResult ShowMailBox([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/ShowMailBox.ps1");
                var results = ps.AddScript(scriptText).AddParameter("userLogin", user.name).Invoke();
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

        [HttpPost("RemoveFromGroup")]
        public ActionResult RemoveFromGroup([FromBody] string data)
        {
            JObject jsonData = JObject.Parse(data);
            UserModel user = jsonData["user"].ToObject<UserModel>();
            string group = jsonData["group"].ToString();
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/RemoveFromGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();

                parameters.Add("grpLogin", group);
                parameters.Add("userLogin", user.name);

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
        public ActionResult UserCreation([FromBody] string data)
        {
            var user = JsonConvert.DeserializeObject<UserModel>(data);
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/UserCreation.ps1");
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
                    return Ok();
                }
                else
                {
                    return BadRequest(final);
                }
            }
        }
    }

}
