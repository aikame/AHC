using DBC.Controllers;
using DBC.Interfaces;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using DBC.Models.Shared;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

namespace DBC.Services
{
    public class SearchService : ISearchService
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<SearchService> _logger;
        public SearchService(ILogger<SearchService> logger, ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }
        private async Task<SearchResponse<T>> SearchAsync<T>(string? index, string sortField, string? query, int? size) where T : class
        {
            if (string.IsNullOrEmpty(sortField))
            {
                throw new ArgumentNullException(nameof(sortField), "Поле для сортировки должно быть указано.");
            }
            var response = await _elasticsearchClient.SearchAsync<T>(s =>
            {
                s.Index(!string.IsNullOrEmpty(index) ? Indices.Index(index) : Indices.All);

                if (!string.IsNullOrEmpty(query))
                {
                    s.Query(q => q
                        .MultiMatch(mm => mm.Query(query))
                    );
                }

                s.Sort(ss => ss
                    .Field(sortField, f => f.Order(SortOrder.Desc))
                );

                s.Size(size);
            });

            return response;
        }

        private async Task<T?> SearchOneAsync<T>(string? index, string sortField, string? query, int? size) where T : class
        {
            var response = await SearchAsync<T>(index, sortField, query, size);

            if (!response.IsValidResponse)
            {
                _logger.LogError(response.ElasticsearchServerError.ToString());
                return null;
            }
            return response.Documents.FirstOrDefault();
        }

        public async Task<IReadOnlyCollection< ElasticProfileModel>?> SearchProfile(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchAsync<ElasticProfileModel>("profiles", "created", query, size);

            if (!response.IsValidResponse)
                return null;

            return response.Documents;
        }
        public async Task<ElasticProfileModel?> SearchOneProfile(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchOneAsync<ElasticProfileModel>("profiles", "created", query, size);

            return response;
        }
        public async Task<IReadOnlyCollection<ElasticComputerModel>?> SearchComputer(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchAsync<ElasticComputerModel>("computers", "updated", query, size);

            if (!response.IsValidResponse)
                return null;

            return response.Documents;
        }
        public async Task<ElasticComputerModel?> SearchOneComputer(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchOneAsync<ElasticComputerModel>("computers", "updated", query, size);

            return response;
        }

        public async Task<ElasticComputerModel?> SearchDomainController([FromQuery] string domain)
        {
            var response = await _elasticsearchClient.SearchAsync<ElasticComputerModel>(s => s
                .Index("computers")
                .Query(q => q
                    .Bool(b => b
                        .Must(m =>
                                m.Term(t => t.Field(f => f.DomainName.Suffix("keyword")).Value(domain))
                        )
                        .Filter(f => f
                            .Term(t => t.Field(ff => ff.ComputerRole).Value(5))
                        )
                    )
                )
                .Size(1)
            );

            if (!response.IsValidResponse)
                return null;
            var controller = response.Documents.FirstOrDefault();
            if (controller == null)
                return null;

            return controller;
        }

        public async Task<IReadOnlyCollection<DomainModel>?> Searchdomain(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchAsync<DomainModel>("domains", "forest.keyword", query, size);

            if (!response.IsValidResponse)
                return null;

            return response.Documents;
        }

        public async Task<IReadOnlyCollection<ElasticGroupModel>?> SearchGroup(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchAsync<ElasticGroupModel>("groups", "name.keyword", query, size);

            if (!response.IsValidResponse)
                return null;

            return response.Documents;
        }

        public async Task<GroupModel?> SearchOneGroup(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchOneAsync<GroupModel>("groups", "name.keyword", query, 1);

            return response;
        }

        public async Task<Dictionary<string,List<JsonObject>>?> SearchAll(string? query, int? size, bool? fullcomp)
        {
            var response = await SearchAsync<JsonObject>("*", "id.keyword", query, size);

            if (!response.IsValidResponse)
                return null;

            var grouped = response.Hits
                .GroupBy(hit => hit.Index)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(hit => hit.Source).ToList()
                );

            return grouped;
        }
    }
}
