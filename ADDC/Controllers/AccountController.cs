using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using ADDC.Interfaces;
using ADDC.Models;

namespace ADDC.Controllers
{
    [ApiController]
    [Route("/")]
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        public AccountController(ILogger<AccountController> logger, IAccountService service)
        {
            _accountService = service;
            _logger = logger;
        }


        [HttpGet("GetInfo")]
        public async Task<IActionResult> GetInfo([FromQuery] string samAccountName)
        {
            try
            {
                var result = await _accountService.GetInfo(samAccountName);
                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest(result);
            }
            catch (Exception e)
            {
                _logger.LogError("[GetInfo] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("BanUser")]
        public async Task<IActionResult> BanUser([FromBody] ADAccountModel user)
        {
            _logger.LogInformation($"[BanUser]: \n{user.SamAccountName}");
            try
            {
                var result = await _accountService.BanUser(user);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }

        }

        [HttpPost("UnbanUser")]
        public async Task<IActionResult> UnbanUser([FromBody] ADAccountModel user)
        {
            _logger.LogInformation($"[UnbanUser]: \n{user.SamAccountName}");
            try
            {
                var result = await _accountService.UnbanUser(user);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                return Problem(e.Message);
            }
        }

        [HttpPost("Authentication")]
        public async Task<IActionResult> Authentication([FromBody] JObject data)
        {
            try
            {
                string user = data["user"].ToString();
                var password = data["password"].ToString();

                _logger.LogInformation($"[Authentication]: \n{user}");
                var result = await _accountService.Authentication(user, password);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"[Authentication] Authentication internal error.");
                return Problem(e.Message);
            }

        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ADAccountModel user)
        {
            try
            {
                var result = await _accountService.ChangePassword(user);
                return result is not null ? Ok(result) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[ChangePassword] Exception: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("CreateMailBox")]
        public async Task<IActionResult> CreateMailBox([FromBody] ADAccountModel user)
        {
            try
            {
                var result = await _accountService.CreateMailBox(user);

                return result is not null ? Content(result.ToString(), "application/json") : BadRequest();

            }
            catch (Exception e)
            {
                _logger.LogError("[CreateMailBox] Exception: " + e.Message);
                return Problem(e.Message);
            }

        }
        [HttpPost("HideMailBox")]
        public async Task<IActionResult> HideMailBox([FromBody] ADAccountModel user)
        {
            _logger.LogInformation($"[HideMailBox]: \n{user}");
            try
            {
                var result = await _accountService.HideMailBox(user);

                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"[HideMailBox]: {e.Message}");
                return Problem(e.Message);
            }
        }

        [HttpPost("ShowMailBox")]
        public async Task<IActionResult> ShowMailBox([FromBody] ADAccountModel user)
        {
            _logger.LogInformation($"[ShowMailBox]: \n{user}");
            try
            {
                var result = await _accountService.ShowMailBox(user);

                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError($"[ShowMailBox]: {e.Message}");
                return Problem(e.Message);
            }
        }

        [HttpPost("UserCreation")]
        public async Task<IActionResult> UserCreation([FromBody] UserModel user)
        {
            try
            {
                var result = await _accountService.Create(user);

                return result is not null ? Content(JsonConvert.SerializeObject(user), "application/json") : BadRequest();

            }
            catch (Exception e)
            {
                _logger.LogError("[UserCreation] Exception: " + e.Message);
                return Problem(e.Message);
            }

        }
    }
}
