using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;

namespace DBC.Interfaces
{
    public interface IComputerService
    {
        Task<ElasticComputerModel?> AddComputer(ComputerModel computer);
        Task<ElasticComputerModel?> UpdateComputer(ComputerModel computer);
        Task<bool> DeleteComputer(string id);
        Task<bool> Reindexate();
    }
}
