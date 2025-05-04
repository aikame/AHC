using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IComputerInfoService
    {
        Task<JObject> CollectInfo();
        Task<bool> CollectAndSendInfo();
    }
}
