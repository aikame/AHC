using Backend.Models.Data;

namespace Backend.Interfaces
{
    public interface IAccountService
    {
        Task<ADAccountModel?> Create(ProfileModel profile, DomainModel domain);
        Task<ADAccountModel?> Get(ADAccountModel user);
        Task<bool> Ban(ADAccountModel account);
        Task<bool> Unban(ADAccountModel account);
        Task<string?> ChangePassword(ADAccountModel data);
        Task<bool> CreateMailBox(ADAccountModel account);
        Task<bool> HideMailBox(ADAccountModel account);
        Task<bool> ShowMailBox(ADAccountModel account);
        Task<bool> Authentication(ADAccountModel account, string password);

    }
}
