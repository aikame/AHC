using ADDC.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Speech.Synthesis;
using ADDC.Models.Data;
using Backend.Models.Requests.Group;

namespace ADDC.Controllers
{
    public class GroupController : Controller
    {
        private readonly ILogger<GroupController> _logger;
        private readonly IGroupService _groupService;
        public GroupController(ILogger<GroupController> logger, IGroupService service)
        {
            _groupService = service;
            _logger = logger;
        }


        [HttpGet("GetGroupMembers")]
        public async Task<IActionResult> GetGroupMembers([FromQuery] string group)
        {
            try
            {
                GroupModel groupModel = new GroupModel();
                groupModel.Name = group;
                var result = await _groupService.GetGroupMembers(groupModel);
                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest(result);
            }
            catch (Exception e)
            {
                _logger.LogError("[GetInfo] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupModel group)
        {
            _logger.LogInformation($"[CreateGroup]: \n {group.Name}");

            try
            {
                var result = await _groupService.CreateGroup(group);

                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateGroup] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }
        }
        [HttpPost("AddToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] AddToGroupRequest data)
        {
            _logger.LogInformation($"[AddToGroup]: \n{data.ToString()}");
            try
            {

                var result = await _groupService.AddToGroup(data.user, data.group);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[AddToGroup] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }

        }

        [HttpPost("RemoveFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] RemoveFromGroupRequest data)
        {
            _logger.LogInformation($"[RemoveFromGroup]: \n{data.ToString()}");
            try
            {
                var result = await _groupService.RemoveFromGroup(data.user, data.group);
                return result ? Ok() : BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogError("[RemoveFromGroup] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }

        }
    }
}
