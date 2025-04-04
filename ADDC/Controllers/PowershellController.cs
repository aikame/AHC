
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
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using ADDC.Services;

namespace ADDC.Controllers
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
            return true;
        }
    }
    [ApiController]
    [Route("/")]
    public class PowershellController : Controller
    {
        private readonly PowershellSessionPoolService _sessionPool;
        private readonly ILogger<PowershellController> _logger;

        public PowershellController(PowershellSessionPoolService sessionPool, ILogger<PowershellController> logger)
        {
            _sessionPool = sessionPool;
            _logger = logger;
        }


        [HttpPost("GetInfo")]
        public ActionResult GetInfo([FromBody] JObject data)
        {
            _logger.LogInformation($"[Getinfo]: \n{data}");
            var userName = data["login"].ToString();

            string result = _sessionPool.ExecuteFunction("GetUserInfo", ("UserLogin", userName));

            try
            {
                JObject jsonData = JObject.Parse(result);
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

                return Content(JsonConvert.SerializeObject(userModel));
            }
            catch (Exception e)
            {
                _logger.LogError("[GetInfo] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }

        [HttpGet("GetAppInfo")]
        public ActionResult GetAppInfo()
        {
            _logger.LogInformation($"GetAppInfo");
            
            string result = _sessionPool.ExecuteFunction("GetAppInfo");

            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[GetAppInfo] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }
        [HttpGet("GetGroupMembers")]
        public ActionResult GetGroupMembers([FromQuery] string group)
        {
            _logger.LogInformation($"[GetGroupMembers]: {group}");
            string result = _sessionPool.ExecuteFunction("GetGroupMembers", ("GroupID", group));
            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[GetGroupMembers] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }

        [HttpGet("GetComputerInfo")]
        public ActionResult GetComputerInfo([FromQuery] string data)
        {
            _logger.LogInformation($"[GetComputerInfo]: {data}");
            string result = _sessionPool.ExecuteFunction("CollectInfo");
            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[GetComputerInfo] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }

        [HttpPost("BanUser")]
        public ActionResult BanUser([FromBody] JObject data)
        {
            _logger.LogInformation($"[BanUser]: \n{data}");
            var user = data.ToObject<UserModel>();
            string result = _sessionPool.ExecuteFunction("BanUser",("UserLogin", user.Name));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("CreateGroup")]
        public ActionResult CreateGroup([FromBody] JObject data)
        {
            _logger.LogInformation($"[CreateGroup]: \n{data.ToString()}");
            string result = _sessionPool.ExecuteFunction("CreateGroup", ("grpName", data["Name"].ToString()),("Description", data["Description"].ToString()));
            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateGroup] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }
        [HttpPost("UnbanUser")]
        public ActionResult UnbanUser([FromBody] JObject data)
        {
            _logger.LogInformation($"[UnbanUser]: \n{data.ToString()}");
            var user = data.ToObject<UserModel>();
            string result = _sessionPool.ExecuteFunction("UnbanUser", ("UserLogin", user.Name));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("AddToGroup")]
        public ActionResult AddToGroup([FromBody] JObject data) {
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();

            _logger.LogInformation($"[AddToGroup]: \n{data.ToString()}");
            string result = _sessionPool.ExecuteFunction("AddToGroup", ("userID", user.Name),("grpID",group));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("Authentication")]
        public ActionResult Authentication([FromBody] JObject data)
        {
            string user = data["user"].ToString();
            var password = data["password"].ToString();

            _logger.LogInformation($"[Authentication]: \n{user}");
            string result = _sessionPool.ExecuteFunction("Authentication", ("username", user), ("password", password));

            if (result == "200")
            {
                return Ok();
            } 
            else if (result == "403"){           
                _logger.LogError($"[Authentication] {user} not in admin group");
                return BadRequest(result);
            }
            else
            {
                _logger.LogError($"[Authentication] Authentication error. Wrong input data");
                return BadRequest(result);
            }
            //return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("ChangePassword")]
        public ActionResult ChangePassword([FromBody] JObject data)
        {
            UserModel user = data["user"].ToObject<UserModel>();
            string password = data["password"].ToString();

            _logger.LogInformation($"[ChangePassword]: \n{user}");
            string result = _sessionPool.ExecuteFunction("ChangePassw", ("userID", user.Name), ("newPasswd", password));

            return result == "200" ? Ok() : BadRequest(result);
            
        }

        [HttpPost("CreateMailBox")]
        public ActionResult CreateMailBox([FromBody] JObject data)
        {
            _logger.LogInformation($"[CreateMailBox]: \n{data.ToString()}");
            string result = _sessionPool.ExecuteFunction("CreateMailBox", ("userLogin", data["Name"].ToString()));
            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateMailBox] JSON Parse Error: " + e.Message);
                return BadRequest("Ошибка обработки данных");
            }
        }

        [HttpPost("HideMailBox")]
        public ActionResult HideMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();

            _logger.LogInformation($"[HideMailBox]: \n{user}");
            string result = _sessionPool.ExecuteFunction("HideMailBox", ("userLogin", user.Name));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("ShowMailBox")]
        public ActionResult ShowMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();

            _logger.LogInformation($"[ShowMailBox]: \n{user}");
            string result = _sessionPool.ExecuteFunction("ShowMailBox", ("userLogin", user.Name));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("RemoveFromGroup")]
        public ActionResult RemoveFromGroup([FromBody] JObject data)
        {
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();

            _logger.LogInformation($"[RemoveFromGroup]: \n{data.ToString()}");
            string result = _sessionPool.ExecuteFunction("RemoveFromGroup", ("userLogin", user.Name), ("grpLogin", group));

            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("UserCreation")]
        public ActionResult UserCreation([FromBody] JObject data)
        {
            _logger.LogInformation($"[UserCreation]: \n{data}");
            var user = data.ToObject<UserModel>();

            string result = _sessionPool.ExecuteFunction("UserCreation", 
                ("name", user.Name), 
                ("surname", user.SurName), 
                ("midname", user.Patronymic), 
                ("city", user.City), 
                ("company", user.Company), 
                ("department", user.Department),
                ("appointment", user.Appointment));
            try
            {
                JObject jsonData = JObject.Parse(result);

                return Content(JsonConvert.SerializeObject(jsonData));
            }
            catch (Exception e)
            {
                _logger.LogError("[UserCreation] JSON Parse Error: " + e.Message + " | " + result);
                return BadRequest("Ошибка обработки данных");
            }
        }
    }

}
