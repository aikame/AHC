using Backend.Interfaces;
using Backend.Models;
using Backend.Models.Data;
using Backend.Models.Requests.Account;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Management.Automation.Language;
using System.Text.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class AccountController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        public AccountController(ILogger<AccountController> logger, IAccountService accountService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("AccountController");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
            _accountService = accountService;
        }
        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.Get(acc);
                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[GetInfo]: " + e.Message);
                return Problem(e.Message);
            }
        }
        [HttpGet("BanUser")]
        public async Task<IActionResult> BanUser([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.Ban(acc);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[BanUser]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("UnbanUser")]
        public async Task<IActionResult> UnbanUser([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.Unban(acc);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[UnbanUser]: " + e.Message);
                return Problem(e.Message);
            }
        }
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ADAccountModel acc)
        {
            try
            {
                var result = await _accountService.ChangePassword(acc);
                JObject response = new JObject();
                response["password"] = result;
                return result is not null ? Ok(JsonConvert.SerializeObject(response)) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[ChangePassword]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("CreateMailBox")]
        public async Task<IActionResult> CreateMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.CreateMailBox(acc);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateMailBox]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("HideMailBox")]
        public async Task<IActionResult> HideMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.HideMailBox(acc);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateMailBox]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("ShowMailBox")]
        public async Task<IActionResult> ShowMailBox([FromQuery] string id, [FromQuery] string domain)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = id, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.ShowMailBox(acc);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[ShowMailBox]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> UserCreation([FromBody] ProfileModel user, [FromQuery] string domain, [FromQuery] bool mail)
        {
            try
            {
                var domainModel = new DomainModel { Forest = domain };
                var acc = await _accountService.Create(user, domainModel);
                if (acc is null) return BadRequest();

                if (mail)
                {
                    var mailRes = _accountService.CreateMailBox(acc);
                }
                return acc is not null ? Content(JsonConvert.SerializeObject(acc)) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[UserCreation]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("Authentication")]
        public async Task<IActionResult> Authentication([FromBody] AuthenticationRequest data)
        {
            try
            {
                string[] userParts = data.user.Split('\\');
                string username = userParts.Length > 1 ? userParts[1] : userParts[0];
                string domain = userParts.Length > 1 ? userParts[0] : "";

                var acc = new ADAccountModel { SamAccountName = username, Domain = new DomainModel { Forest = domain } };
                var result = await _accountService.Authentication(acc,data.password);

                return result ? Ok() : BadRequest(); 
            }
            catch (Exception e)
            {
                _logger.LogError("[Authentication]: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}
