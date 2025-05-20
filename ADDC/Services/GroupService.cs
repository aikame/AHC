using ADDC.Interfaces;
using ADDC.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ADDC.Services
{
    public class GroupService : IGroupService
    {
        private readonly IPowershellSessionPoolService _sessionPool;
        private readonly ILogger<GroupService> _logger;
        public GroupService(IPowershellSessionPoolService sessionPool, ILogger<GroupService> logger)
        {
            _sessionPool = sessionPool;
            _logger = logger;
        }
        public async Task<JObject?> GetGroupMembers(string group)
        {
            _logger.LogInformation($"[GetGroupMembers]: {group}");
            var result = await _sessionPool.ExecuteFunction("GetGroupMembers", ("GroupID", group));
            try
            {
                JObject jsonData = JObject.Parse(result);

                return jsonData;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetGroupMembers] JSON Parse Error: " + e.Message);
                return null;
            }
        }
        public async Task<GroupModel?> CreateGroup(GroupModel group)
        {
            _logger.LogInformation($"[CreateGroup]: \n{group.Name}");
            var result = await _sessionPool.ExecuteFunction("CreateGroup", ("grpName", group.Name), ("Description", group.Description));

            try
            {
                JObject jsonData = JObject.Parse(result);
                group = jsonData.ToObject<GroupModel>();
                return group;
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateGroup] JSON Parse Error: " + e.Message);
                return null;
            }
        }

        public async Task<bool> AddToGroup(UserModel user, string group)
        {
            _logger.LogInformation($"[AddToGroup]: \n{user.Name}");
            var result = await _sessionPool.ExecuteFunction("AddToGroup", ("userID", user.Name), ("grpID", group));
            return result == "200" ? true : false;
        }

        public async Task<bool> RemoveFromGroup(UserModel user, string group)
        {
            _logger.LogInformation($"[AddToGroup]: \n{user.Name}");
            var result = await _sessionPool.ExecuteFunction("RemoveFromGroup", ("userID", user.Name), ("grpID", group));
            return result == "200" ? true : false;
        }
    }
}
