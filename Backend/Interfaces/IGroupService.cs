using Backend.Models;
using Newtonsoft.Json.Linq;

namespace Backend.Interfaces
{
    public interface IGroupService
    {
        Task<bool> AddToGroup(ADAccountModel user, GroupModel group);
        Task<bool> RemoveFromGroup(ADAccountModel user, GroupModel group);
        Task<GroupModel?> CreateGroup(GroupModel group);
        Task<JObject?> GetGroupMembers(GroupModel group);

    }
}
