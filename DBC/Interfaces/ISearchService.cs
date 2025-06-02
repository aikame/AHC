using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using DBC.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace DBC.Interfaces
{
    public interface ISearchService
    {
        Task<IReadOnlyCollection<ElasticProfileModel>?> SearchProfile(string? query, int? size, bool? fullcomp);
        Task<ElasticProfileModel?> SearchOneProfile(string? query, int? size, bool? fullcomp);
        Task<IReadOnlyCollection<ElasticComputerModel>?> SearchComputer(string? query, int? size, bool? fullcomp);
        Task<ElasticComputerModel?> SearchOneComputer(string? query, int? size, bool? fullcomp);
        Task<ElasticComputerModel?> SearchDomainController([FromQuery] string domain);
        Task<IReadOnlyCollection<DomainModel>?> Searchdomain(string? query, int? size, bool? fullcomp);
        Task<IReadOnlyCollection<ElasticGroupModel>?> SearchGroup(string? query, int? size, bool? fullcomp);
        Task<GroupModel?> SearchOneGroup(string? query, int? size, bool? fullcomp);
        Task<Dictionary<string, List<JsonObject>>?> SearchAll(string? query, int? size, bool? fullcomp);

    }
}
