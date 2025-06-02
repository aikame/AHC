using DBC.Data;
using DBC.Interfaces;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Principal;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/group")]
    public class GroupController : ControllerBase
    {
        private readonly ILogger<GroupController> _logger;
        private readonly IGroupService _groupService;
        public GroupController(ILogger<GroupController> logger,IGroupService groupService)
        {
            _logger = logger;
            _groupService = groupService;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddGroup([FromBody] GroupModel group)
        {
            _logger.LogInformation("[AddGroup]: " + JsonConvert.SerializeObject(group));
            if (group == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _groupService.AddGroup(group);
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
                _logger.LogError("[AddGroup]: " + e.Message);
                return Problem(e.Message);
            }
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateGroup([FromBody] GroupModel group)
        {
            _logger.LogInformation("[UpdateGroup]: " + JsonConvert.SerializeObject(group));
            if (group == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _groupService.UpdateGroup(group);
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
                _logger.LogError("[UpdateGroup]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteGroups([FromQuery] string id)
        {
            _logger.LogInformation("[DeleteGroups]: " + JsonConvert.SerializeObject(id));
            if (id == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _groupService.DeleteGroups(id);
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
                _logger.LogError("[DeleteGroups]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            _logger.LogInformation("[reindexate]: ");
            try
            {
                var result = await _groupService.Reindexate();
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
