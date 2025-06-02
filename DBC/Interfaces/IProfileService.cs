using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;

namespace DBC.Interfaces
{
    public interface IProfileService
    {
        Task<ElasticProfileModel?> AddProfile(ProfileModel profile);
        Task<ElasticProfileModel?> UpdateProfile(ProfileModel profile);
        Task<bool> DeleteProfile(string id);
        Task<bool> Reindexate();
    }
}
