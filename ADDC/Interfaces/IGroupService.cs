using ADDC.Models;
using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IGroupService
    {
        Task<JObject?> GetGroupMembers(string group);
        Task<GroupModel?> CreateGroup(GroupModel group);
        Task<bool?> AddToGroup(UserModel user, string group);
    }
}
