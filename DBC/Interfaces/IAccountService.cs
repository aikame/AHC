using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;

namespace DBC.Interfaces
{
    public interface IAccountService
    {
        Task<ElasticProfileModel?> AddADAccount(ADAccountModel account);
        Task<ElasticProfileModel?> UpdateADAccount(ADAccountModel account);
    }
}
