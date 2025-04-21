using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.Shared;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/search")]
    public class SearchController : ControllerBase
    {
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<SearchController> _logger;
        public SearchController(ILogger<SearchController> logger, ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _logger = logger;
        }

        private async Task<SearchResponse<T>> SearchAsync<T>(string index, string? query) where T : class 
        {
            var response = string.IsNullOrWhiteSpace(query)
            ? await _elasticsearchClient.SearchAsync<T>(s => s
                .Index(index)
                .Sort(ss => ss
                    .Field(new Field("created"), sort => sort.Order(SortOrder.Desc))
                )
                .Size(10)
            )
            : await _elasticsearchClient.SearchAsync<T>(s => s
                .Index("profiles")
                .Query(q => q
                    .MultiMatch(mm => mm
                        .Query(query)
                    )
                )
            );
            return response;
        }
        [HttpGet("profile")]
        public async Task<IActionResult> SearchProfile([FromQuery] string? query)
        {
            var response = await SearchAsync<ElasticProfileModel>("profiles",query);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }
        [HttpGet("computer")]
        public async Task<IActionResult> SearchComputer([FromQuery] string? query)
        {
            var response = await SearchAsync<ComputerModel>("computers", query);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }
        [HttpGet("group")]
        public async Task<IActionResult> SearchGroup([FromQuery] string? query)
        {
            var response = await SearchAsync<GroupModel>("groups", query);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }

        [HttpGet("all")]
        public async Task<IActionResult> SearchAll([FromQuery] string? query)
        {
            var response = await SearchAsync<JsonObject>("*", query);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            var grouped = response.Hits
                .GroupBy(hit => hit.Index)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(hit => hit.Source).ToList()
                );

            return Ok(grouped);
        }
    }
    


}
