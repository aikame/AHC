using Backend.Interfaces;
using Backend.Models.Data;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices.ActiveDirectory;
using System.Text.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class ProfileController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileService _profileService;
        private readonly IAccountService _accountService;
        public ProfileController(ILogger<ProfileController> logger, IAccountService accountService, IProfileService profileService,IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("ProfileController");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
            _profileService = profileService;
            _accountService = accountService;
        }
        [HttpPost("CreateProfile")]
        public async Task<IActionResult> ProfileCreation([FromBody] ProfileModel user, [FromQuery] string? domain, [FromQuery] bool blocked = false)
        {
            try
            { 
                var res= await _profileService.Create(user);
                if (res is null) { return BadRequest("Wrong input data"); }
                user.Id = res.Value;
                if (user.ADreq)
                {
                    DomainModel domainModel = new DomainModel { Forest = domain};
                    var acc = await _accountService.Create(user, domainModel);
                    var mailRes = await _accountService.CreateMailBox(acc);
                    if (blocked) { _accountService.Ban(acc); }
                    return Ok(user.Id.ToString());
                }
                return Content(user.Id.ToString());
            }
            catch (Exception e)
            {
                _logger.LogError("[ProfileCreation]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("FireUser")]
        public async Task<IActionResult> FireUser([FromBody] ProfileModel profile)
        {
            try
            {
                var result = await _profileService.FireUser(profile);
                return result ? Ok() : BadRequest(); 
            }
            catch (Exception e)
            {
                _logger.LogError("[FireUser]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("ReturnUser")]
        public async Task<IActionResult> ReturnUser([FromBody] ProfileModel profile)
        {
            try
            {
                var result = await _profileService.ReturnUser(profile);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[ReturnUser]: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}
