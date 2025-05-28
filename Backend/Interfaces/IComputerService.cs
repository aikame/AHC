using Backend.Models.Data;
using Newtonsoft.Json.Linq;

namespace Backend.Interfaces
{
    public interface IComputerService
    {
        Task<ComputerModel?> FindDCinDomain(DomainModel domain);
        Task<bool> CollectComputerInfo(ComputerModel computer);
        Task<bool> CheckComputer(ComputerModel computer);
        Task<JObject?> GetAppInfo(ComputerModel computer);
        Task<List<String>?> GetDomainList();
    }
}
