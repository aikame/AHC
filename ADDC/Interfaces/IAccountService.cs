using ADDC.Models;
using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IAccountService
    {
        Task<ADAccountModel?> GetInfo(string SamAccountName);
        Task<bool> BanUser(UserModel user);
        Task<bool> UnbanUser(UserModel user);
        Task<bool> Authentication(string user, string password);
        Task<string> ChangePassword(UserModel user);
        Task<JObject> CreateMailBox(UserModel user);
        Task<bool> HideMailBox(UserModel user);
        Task<bool> ShowMailBox(UserModel user);
        Task<ADAccountModel?> Create(UserModel user);
    }
}
