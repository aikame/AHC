
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


namespace ADDC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PowershellController : Controller
    {
        [HttpPost("GetInfo")]
        public ActionResult GetInfo(HttpContext context)
        {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();

                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/GetUserInfo.ps1");
                var results = ps.AddScript(scriptText).AddParameter("UserLogin", context.Request.Query["UserLogin"]).Invoke();
                string final = "";
                foreach (var result in results)
                {

                    final += result.ToString();
                }
                return Content(final);
            }
        }
        [HttpPost("BanUser")]
        public ActionResult BanUser([FromBody] UserModel user)
        {
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
        public ActionResult UnbanUser([FromBody] UserModel user)
        {
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
        public ActionResult AddToGroup([FromBody] UserModel user, [FromBody] string groupID) {
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string scriptText = System.IO.File.ReadAllText("../../../PowershellFunctions/AddToGroup.ps1");
                System.Collections.IDictionary parameters = new Dictionary<string, string>();
                parameters.Add("grpID", groupID);
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

    }

}
