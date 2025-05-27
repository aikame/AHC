using Backend.Models;

namespace Backend.Interfaces
{
    public interface IProfileService
    {
        Task<Guid?> Create(ProfileModel profile);
        Task<ProfileModel?> Get(string query);
        Task<bool> Update(ProfileModel profile);
        Task<bool> FireUser(ProfileModel profile);
        Task<bool> ReturnUser(ProfileModel profile);

    }
}
