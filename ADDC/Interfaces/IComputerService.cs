using ADDC.Models;
using Newtonsoft.Json.Linq;

namespace ADDC.Interfaces
{
    public interface IComputerService
    {
        Task<JObject?> GetAppInfo();
        Task<ComputerModel?> GetInfo();
    }
}
