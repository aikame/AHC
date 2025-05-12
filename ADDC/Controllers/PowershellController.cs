
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
using System.Text;
using ADDC.Interfaces;

namespace ADDC.Controllers
{
    
    [ApiController]
    [Route("/")]
    public class PowershellController : Controller
    {
        private readonly IPowershellSessionPoolService _sessionPool;
        private readonly IExchangePowershellSessionPoolService _exchangeSessionPool;
        private readonly ILogger<PowershellController> _logger;

        public PowershellController(IPowershellSessionPoolService sessionPool, IExchangePowershellSessionPoolService exchangeSessionPool, ILogger<PowershellController> logger)
        {
            _exchangeSessionPool = exchangeSessionPool;
            _sessionPool = sessionPool;
            _logger = logger;
        }


        [HttpPost("GetInfo")]
        public ActionResult GetInfo([FromBody] JObject data)
        {
            _logger.LogInformation($"[Getinfo]: \n{data}");
            var userName = data["login"].ToString();

            var func = _sessionPool.ExecuteFunction("GetUserInfo", ("UserLogin", userName));
            string result = func.Result;
            try
            {
                JObject jsonData = JObject.Parse(result);
                string StringPasswordLastSet = jsonData["PasswordLastSet"]?.ToString();
                var PasswordLastSet = StringPasswordLastSet.Length > 1 ? DateTime.Parse(StringPasswordLastSet) : DateTime.MinValue;

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
            
            var  func = _sessionPool.ExecuteFunction("GetAppInfo");
            string result = func.Result;

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
            var func = _sessionPool.ExecuteFunction("GetGroupMembers", ("GroupID", group));
            string result = func.Result;
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
            var func = _sessionPool.ExecuteFunction("CollectInfo");
            string result = func.Result;
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
            var func = _sessionPool.ExecuteFunction("BanUser",("UserLogin", user.Name));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("CreateGroup")]
        public ActionResult CreateGroup([FromBody] JObject data)
        {
            _logger.LogInformation($"[CreateGroup]: \n{data.ToString()}");
            var func = _sessionPool.ExecuteFunction("CreateGroup", ("grpName", data["Name"].ToString()),("Description", data["Description"].ToString()));
            string result = func.Result;
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
            var func = _sessionPool.ExecuteFunction("UnbanUser", ("UserLogin", user.Name));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("AddToGroup")]
        public ActionResult AddToGroup([FromBody] JObject data) {
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();

            _logger.LogInformation($"[AddToGroup]: \n{data.ToString()}");
            var func = _sessionPool.ExecuteFunction("AddToGroup", ("userID", user.Name),("grpID",group));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("Authentication")]
        public ActionResult Authentication([FromBody] JObject data)
        {
            string user = data["user"].ToString();
            var password = data["password"].ToString();

            _logger.LogInformation($"[Authentication]: \n{user}");
            var func = _sessionPool.ExecuteFunction("Authentication", ("username", user), ("password", password));
            string result = func.Result;
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
            try
            {
                _logger.LogInformation(data.ToString());
                UserModel user = data["user"].ToObject<UserModel>();
                string password = GeneratePassword(12);
                _logger.LogInformation($"[ChangePassword]: \n{user}");
                var func = _sessionPool.ExecuteFunction("ChangePassw", ("userID", user.Name), ("newPasswd", password));
                string result = func.Result;
                return result == "200" ? Ok(password) : BadRequest(result);
            }
            catch (Exception e){
                _logger.LogError("[ChangePassword] Exception: " + e.Message);
                return BadRequest(e);
            }
            
            
        }

        [HttpPost("CreateMailBox")]
        public ActionResult CreateMailBox([FromBody] JObject data)
        {
            try
            {
                
                var userName = data["name"].ToString();
                _logger.LogInformation("[CreateMailBox] " + userName);
                var func = _exchangeSessionPool.ExecuteFunction("CreateMailBox", ("userLogin", userName));
                string result = func.Result;
                if (result == "404")
                {
                    _logger.LogError("[CreateMailBox] 404 Error");
                    return BadRequest();
                }
                var jsonData = JObject.Parse(result);
                return Content(jsonData.ToString(), "application/json");
            }
            catch (Exception e) {
                _logger.LogError("[CreateMailBox] Exception: " + e.Message);
                return BadRequest(e.Message);
            }

        }

        [HttpPost("HideMailBox")]
        public ActionResult HideMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();

            _logger.LogInformation($"[HideMailBox]: \n{user}");
            var func = _sessionPool.ExecuteFunction("HideMailBox", ("userLogin", user.Name));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("ShowMailBox")]
        public ActionResult ShowMailBox([FromBody] JObject data)
        {
            var user = data.ToObject<UserModel>();

            _logger.LogInformation($"[ShowMailBox]: \n{user}");
            var func = _sessionPool.ExecuteFunction("ShowMailBox", ("userLogin", user.Name));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }

        [HttpPost("RemoveFromGroup")]
        public ActionResult RemoveFromGroup([FromBody] JObject data)
        {
            UserModel user = data["user"].ToObject<UserModel>();
            string group = data["group"].ToString();

            _logger.LogInformation($"[RemoveFromGroup]: \n{data.ToString()}");
            var func = _sessionPool.ExecuteFunction("RemoveFromGroup", ("userLogin", user.Name), ("grpLogin", group));
            string result = func.Result;
            return result == "200" ? Ok() : BadRequest(result);
        }
        string GeneratePassword(int length)
        {
            
            var rand = new Random();
            string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            string nums = "0123456789";
            string spec = "!@#$%^&*()-_=+[]{};:.<>?~";
            string password;
            bool upperCaseHit = false, lowerCaseHit = false, numsHit = false, specHit = false;
            do
            {
                StringBuilder sb = new StringBuilder();
                password = "";
                upperCaseHit = false; lowerCaseHit = false; numsHit = false; specHit = false;
                for (int i = 0; i < length; i++)
                {
                    
                    int type = rand.Next(0, 4);
                    char s;
                    switch (type)
                    {
                        case 0:
                            s = upperCase[rand.Next(upperCase.Length)];
                            upperCaseHit = true;
                            break;
                        case 1:
                            s = lowerCase[rand.Next(lowerCase.Length)];
                            lowerCaseHit = true;
                            break;
                        case 2:
                            s = nums[rand.Next(nums.Length)];
                            numsHit = true;
                            break;
                        case 3:
                            s = spec[rand.Next(spec.Length)];
                            specHit = true;
                            break;
                        default:
                            s = upperCase[rand.Next(upperCase.Length)];
                            upperCaseHit = true;
                            break;
                    }
                    sb.Append(s);
                }
                password = sb.ToString();
            } while (!(upperCaseHit && lowerCaseHit && numsHit && specHit));
            return password;
        }
        [HttpPost("UserCreation")]
        public ActionResult UserCreation([FromBody] JObject data)
        {
            _logger.LogInformation($"[UserCreation]: \n{data}");
            var user = data.ToObject<UserModel>();
            string password = GeneratePassword(12);
            var func = _sessionPool.ExecuteFunction("UserCreation", 
                ("name", user.Name), 
                ("surname", user.SurName), 
                ("midname", user.Patronymic), 
                ("city", user.City), 
                ("company", user.Company), 
                ("department", user.Department),
                ("appointment", user.Appointment),
                ("password", password));
            string result = func.Result;
            try
            {
                JObject jsonData = JObject.Parse(result);
                jsonData["password"] = password;
                _logger.LogInformation($"[UserCreation] Created: {jsonData}");
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
