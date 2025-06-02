using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;

namespace DBC.Interfaces
{
    public interface IGroupService
    {
        Task<ElasticGroupModel?> AddGroup(GroupModel group);
        Task<ElasticGroupModel?> UpdateGroup(GroupModel group);
        Task<bool> DeleteGroups(string id);
        Task<bool> Reindexate();

    }
}
