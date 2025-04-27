using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
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




        [HttpGet("profile")]
        public async Task<IActionResult> SearchProfile([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await SearchAsync<ElasticProfileModel>("profiles","created",query,size);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }
        [HttpGet("oneprofile")]
        public async Task<IActionResult> SearchOneProfile([FromQuery] string query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await SearchAsync<ElasticProfileModel>("profiles", "created", query, size);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents.FirstOrDefault());
        }
        [HttpGet("computer")]
        public async Task<IActionResult> SearchComputer([FromQuery] string? query, [FromQuery] int? size)
        {
            var response = await SearchAsync<ComputerModel>("computers","updated", query, size);

            if (!response.IsValidResponse)
            {
                _logger.LogError(response.ElasticsearchServerError.ToString());
                return StatusCode(500, "Search Error");
            }
                

            return Ok(response.Documents);

        }
        [HttpGet("onecomputer")]
        public async Task<IActionResult> SearchOneComputer([FromQuery] string query)
        {
            var response = await SearchAsync<ComputerModel>("computers", "updated", query, 1);
            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");
            return Ok(response.Documents.FirstOrDefault());
        }

        [HttpGet("domain-controller")]
        public async Task<IActionResult> SearchDomainController([FromQuery] string domain)
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
                return StatusCode(500, "Search Error");
            var controller = response.Documents.FirstOrDefault();
            if (controller == null)
                return NotFound();

            return Ok(controller);
        }


        [HttpGet("group")]
        public async Task<IActionResult> SearchGroup([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await SearchAsync<GroupModel>("groups", "name.keyword", query,size);

            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");

            return Ok(response.Documents);
        }
        [HttpGet("onegroup")]
        public async Task<IActionResult> SearchOneGroup([FromQuery] string query)
        {
            var response = await SearchAsync<GroupModel>("groups", "name.keyword", query, 1);
            if (!response.IsValidResponse)
                return StatusCode(500, "Search Error");
            return Ok(response.Documents.FirstOrDefault());
        }
        [HttpGet("all")]
        public async Task<IActionResult> SearchAll([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await SearchAsync<JsonObject>("*","Id", query, size);

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
