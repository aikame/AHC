using ADDC.Controllers;
using ADDC.Interfaces;
using ADDC.Models.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ADDC.Services
{
    public class AccountService : IAccountService
    {
        private readonly IPowershellSessionPoolService _sessionPool;
        private readonly IExchangePowershellSessionPoolService _exchangeSessionPool;
        private readonly ILogger<AccountService> _logger;
        public AccountService(IPowershellSessionPoolService sessionPool, IExchangePowershellSessionPoolService exchangeSessionPool, ILogger<AccountService> logger)
        {
            _sessionPool = sessionPool;
            _exchangeSessionPool = exchangeSessionPool;
            _logger = logger;
        }
        private string GeneratePassword(int length)
        {

            var rand = new Random();
            string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            string nums = "0123456789";
            string spec = "!@#$%^&*()-_=+[]{};:.<>?~";
            string password;
            bool upperCaseHit = false, lowerCaseHit = false, numsHit = false, specHit = false;
            do
            {
                StringBuilder sb = new StringBuilder();
                password = "";
                upperCaseHit = false; lowerCaseHit = false; numsHit = false; specHit = false;
                for (int i = 0; i < length; i++)
                {

                    int type = rand.Next(0, 4);
                    char s;
                    switch (type)
                    {
                        case 0:
                            s = upperCase[rand.Next(upperCase.Length)];
                            upperCaseHit = true;
                            break;
                        case 1:
                            s = lowerCase[rand.Next(lowerCase.Length)];
                            lowerCaseHit = true;
                            break;
                        case 2:
                            s = nums[rand.Next(nums.Length)];
                            numsHit = true;
                            break;
                        case 3:
                            s = spec[rand.Next(spec.Length)];
                            specHit = true;
                            break;
                        default:
                            s = upperCase[rand.Next(upperCase.Length)];
                            upperCaseHit = true;
                            break;
                    }
                    sb.Append(s);
                }
                password = sb.ToString();
            } while (!(upperCaseHit && lowerCaseHit && numsHit && specHit));
            return password;
        }
        public async Task<ADAccountModel?> GetInfo(string SamAccountName)
        {
            _logger.LogInformation($"GetUser: " + SamAccountName);
            var result = await _sessionPool.ExecuteFunction("GetUserInfo", ("UserLogin", SamAccountName));
            try
            {
                JObject jsonData = JObject.Parse(result);
                string StringPasswordLastSet = jsonData["PasswordLastSet"]?.ToString();
                var PasswordLastSet = StringPasswordLastSet.Length > 1 ? DateTime.Parse(StringPasswordLastSet) : DateTime.MinValue;

                var account = new ADAccountModel
                {
                    DistinguishedName = jsonData["DistinguishedName"].ToString(),
                    SamAccountName = jsonData["SamAccountName"].ToString(),
                    Enabled = jsonData["Enabled"].ToString() == "True",
                    EmailAddress = jsonData["EmailAddress"].ToString(),
                    PasswordLastSet = PasswordLastSet,
                    PasswordExpired = jsonData["PasswordExpired"].ToString() == "True",
                    MemberOf = jsonData["MemberOf"].ToObject<List<string>>()
                };
                return account;
            } catch (Exception e) {
                _logger.LogError("[GetInfo] JSON Parse Error: " + e.Message);
                return null;
            }
        }
        public async Task<bool> BanUser(ADAccountModel user)
        {
            _logger.LogInformation($"[BanUser]: \n{user.SamAccountName}");
            try
            {
                var result = await _sessionPool.ExecuteFunction("BanUser", ("UserLogin", user.SamAccountName));
                return result == "200" ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError($"[BanUser]: {e.Message}");
                return false;
            }
            
        }

        public async Task<bool> UnbanUser(ADAccountModel user)
        {
            try
            {
                var result = await _sessionPool.ExecuteFunction("UnbanUser", ("UserLogin", user.SamAccountName));
                return result == "200" ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError($"[UnbanUser]: {e.Message}");
                return false;
            }
        }

        public async Task<bool> Authentication(string user, string password)
        {
            _logger.LogInformation($"[Authentication]: \n{user}");
            var result = await _sessionPool.ExecuteFunction("Authentication", ("username", user), ("password", password));
            if (result == "200")
            {
                return true;
            }
            else if (result == "403")
            {
                _logger.LogError($"[Authentication] {user} not in admin group");
                return false;
            }
            else
            {
                _logger.LogError($"[Authentication] Authentication error. Wrong input data");
                return false;
            }
        }

        public async Task<string?> ChangePassword(ADAccountModel user)
        {
            try
            {
                string password = GeneratePassword(12);
                _logger.LogInformation($"[ChangePassword]: \n{user}");
                var result = await _sessionPool.ExecuteFunction("ChangePassw", ("userID", user.SamAccountName), ("newPasswd", password));
                return result == "200" ? password : null;
            }
            catch (Exception e)
            {
                _logger.LogError("[ChangePassword] Exception: " + e.Message);
                return e.ToString();
            }
        }

        public async Task<JObject?>CreateMailBox(ADAccountModel user)
        {
            try
            {
                _logger.LogInformation("[CreateMailBox] " + user.SamAccountName);
                var result = await _exchangeSessionPool.ExecuteFunction("CreateMailBox", ("userLogin", user.SamAccountName));
                if (result == "404")
                {
                    _logger.LogError("[CreateMailBox] 404 Error");
                    return null;
                }
                var jsonData = JObject.Parse(result);
                return jsonData;
            }
            catch (Exception e)
            {
                _logger.LogError("[CreateMailBox] Exception: " + e.Message);
                return null;
            }

        }

        public async Task<bool> HideMailBox(ADAccountModel user)
        {
            _logger.LogInformation($"[HideMailBox]: \n{user}");
            try
            {
                var result = await _sessionPool.ExecuteFunction("HideMailBox", ("userLogin", user.SamAccountName));
                return result == "200" ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError($"[HideMailBox]: {e.Message}");
                return false;
            }
        }
        public async Task<bool> ShowMailBox(ADAccountModel user)
        {
            try
            {
                var result = await _sessionPool.ExecuteFunction("ShowMailBox", ("userLogin", user.SamAccountName));
                return result == "200" ? true : false;
            }
            catch (Exception e)
            {
                _logger.LogError($"[ShowMailBox]: {e.Message}");
                return false;
            }
        }
        public async Task<ADAccountModel?> Create(ProfileModel user)
        {
            _logger.LogInformation($"[UserCreation]: \n{user.Name}");
            string password = GeneratePassword(12);
            var result = await _sessionPool.ExecuteFunction("UserCreation",
                ("name", user.Name),
                ("surname", user.SurName),
                ("midname", user.Patronymic),
                ("city", user.City),
                ("company", user.Company),
                ("department", user.Department),
                ("appointment", user.Appointment),
                ("password", password));
            try
            {
                
                JObject jsonData = JObject.Parse(result);
                jsonData["password"] = password;
                ADAccountModel account = jsonData.ToObject< ADAccountModel>();
                _logger.LogInformation($"[UserCreation] Created: {jsonData}");
                return account;
            }
            catch (Exception e)
            {
                _logger.LogError("[UserCreation] JSON Parse Error: " + e.Message + " | " + result);
                return null;
            }
        }
    }
}
