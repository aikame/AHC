using Backend.Interfaces;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class GroupController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<GroupController> _logger;
        private readonly IGroupService _groupService;
        public GroupController(ILogger<GroupController> logger, IGroupService groupService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("GroupController");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
            _groupService = groupService;
        }
        [HttpPost("RemoveFromGroup")]
        public async Task<IActionResult> RemoveFromGroup([FromBody] JObject data)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = data["user"]["name"].ToString(), Domain = new DomainModel { Forest = data["domain"].ToString() } };
                var group = new GroupModel { Name = data["group"].ToString() };
                var result = await _groupService.RemoveFromGroup(acc, group);
                return result ? Ok() : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[RemoveFromGroup]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("AddToGroup")]
        public async Task<IActionResult> AddToGroup([FromBody] JObject data)
        {
            try
            {
                var acc = new ADAccountModel { SamAccountName = data["user"]["name"].ToString(), Domain = new DomainModel { Forest = data["domain"].ToString() } };
                var group = new GroupModel { Name = data["group"].ToString() };
                var result = await _groupService.AddToGroup(acc, group);
                return result ? Ok() : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[AddToGroup]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] JObject data)
        {
            try
            {
               
                var group = data.ToObject<GroupModel>();
                group.Domain = new DomainModel { Forest = data["domain"].ToString() };
                var result = await _groupService.CreateGroup(group);

                return result is not null ? Ok() : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateGroup]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("GetGroupMembers")]
        public async Task<IActionResult> GetGroupMembers([FromQuery] string group, [FromQuery] string domain)
        {
            try
            {

                var groupModel = new GroupModel { Name = group, Domain = new DomainModel { Forest = domain } };
                var result = await _groupService.GetGroupMembers(groupModel);
                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[GetGroupMembers]: " + e.Message);
                return Problem(e.Message);
            }
        }

    }
}
