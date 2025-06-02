using DBC.Data;
using DBC.Interfaces;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileService _profileService;
        public ProfileController(ILogger<ProfileController> logger, IProfileService profile)
        {
            _logger = logger;
            _profileService = profile;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProfile([FromBody] ProfileModel profile)
        {
            _logger.LogInformation("[AddProfile]: " + JsonConvert.SerializeObject(profile));
            if (profile == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _profileService.AddProfile(profile);
                if (result is not null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e) { 
                _logger.LogError("[AddProfile]: " + e.Message);
                return Problem(e.Message);
            }
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileModel profile)
        {
            _logger.LogInformation("[update]: " + JsonConvert.SerializeObject(profile));
            if (profile == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _profileService.UpdateProfile(profile);
                if (result is not null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[update]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProfile([FromQuery] string id)
        {
            _logger.LogInformation("[delete]: " + JsonConvert.SerializeObject(id));
            if (id == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _profileService.DeleteProfile(id);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[delete]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            _logger.LogInformation("[reindexate]: ");
            try
            {
                var result = await _profileService.Reindexate();
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[reindexate]: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}
