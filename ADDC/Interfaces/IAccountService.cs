using ADDC.Models;
using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IAccountService 
    {
        Task<ADAccountModel?> GetInfo(string SamAccountName);
        Task<bool> BanUser(ADAccountModel user);
        Task<bool> UnbanUser(ADAccountModel user);
        Task<bool> Authentication(string user, string password);
        Task<string?> ChangePassword(ADAccountModel user);
        Task<JObject?> CreateMailBox(ADAccountModel user);
        Task<bool> HideMailBox(ADAccountModel user);
        Task<bool> ShowMailBox(ADAccountModel user);
        Task<ADAccountModel?> Create(UserModel user);
    }
}
