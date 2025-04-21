using DBC.Data;
using DBC.Models.Elastic;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;

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
        [HttpGet("profile")]
        public async Task<IActionResult> SearchProfile([FromQuery] string? query)
        {
            var response = string.IsNullOrWhiteSpace(query)
        ? await _elasticsearchClient.SearchAsync<ElasticProfileModel>(s => s
            .Index("profiles")
            .Sort(ss => ss
                .Field(new Field("created"), sort => sort.Order(SortOrder.Desc))
)
            .Size(10)
        )
        : await _elasticsearchClient.SearchAsync<ElasticProfileModel>(s => s
            .Index("profiles")
            .Query(q => q
                .MultiMatch(mm => mm
                    .Query(query)
                )
            )
        );

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }
    }

}
