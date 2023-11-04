
using System.Management.Automation;
using Microsoft.AspNetCore.Mvc;
using System.Management.Automation.Runspaces;

namespace ADDC.Controllers
{
    public class PowershellController : Controller
    {
        [HttpPost]
        public ActionResult ExecuteScript(HttpContext context)
        {
            string final = "";
            using (var ps = PowerShell.Create())
            {
                InitialSessionState iss = InitialSessionState.CreateDefault();
                string script = HttpContext.Request.Query["Script"].ToString();
                var results = ps.AddScript(script).Invoke();
                
                foreach (var result in results)
                {

                    final += result.ToString();
                }
            }
            return Content(final);
        }
    }

}
