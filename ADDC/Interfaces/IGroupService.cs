using ADDC.Models.Data;
using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IGroupService
    {
        Task<JObject?> GetGroupMembers(GroupModel group);
        Task<GroupModel?> CreateGroup(GroupModel group);
        Task<bool> AddToGroup(ADAccountModel user, GroupModel group);
        Task<bool> RemoveFromGroup(ADAccountModel user, GroupModel group);
    }
}
