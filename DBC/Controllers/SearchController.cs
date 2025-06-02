using DBC.Data;
using DBC.Interfaces;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using DBC.Models.Shared;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/search")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;
        public SearchController(ILogger<SearchController> logger, ISearchService searchService)
        {
            _searchService = searchService;
            _logger = logger;
        }


        [HttpGet("profile")]
        public async Task<IActionResult> SearchProfile([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await _searchService.SearchProfile(query, size, fullcomp);

            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
        [HttpGet("oneprofile")]
        public async Task<IActionResult> SearchOneProfile([FromQuery] string query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await _searchService.SearchOneProfile(query, size, fullcomp);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
        [HttpGet("computer")]
        public async Task<IActionResult> SearchComputer([FromQuery] string? query, [FromQuery] int? size)
        {
            var response = await _searchService.SearchComputer(query, size, false);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);

        }
        [HttpGet("onecomputer")]
        public async Task<IActionResult> SearchOneComputer([FromQuery] string query)
        {
            var response = await _searchService.SearchOneComputer(query, 1, false);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }

        [HttpGet("domain-controller")]
        public async Task<IActionResult> SearchDomainController([FromQuery] string domain)
        {
            var response = await _searchService.SearchDomainController(domain);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }

        [HttpGet("domain")]
        public async Task<IActionResult> Searchdomain([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await _searchService.Searchdomain(query, size, fullcomp);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
        [HttpGet("group")]
        public async Task<IActionResult> SearchGroup([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await _searchService.SearchGroup(query, size, fullcomp);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
        [HttpGet("onegroup")]
        public async Task<IActionResult> SearchOneGroup([FromQuery] string query)
        {
            var response = await _searchService.SearchOneGroup(query, 1, false);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
        [HttpGet("all")]
        public async Task<IActionResult> SearchAll([FromQuery] string? query, [FromQuery] int? size, [FromQuery] bool? fullcomp)
        {
            var response = await _searchService.SearchAll(query, size, fullcomp);
            if (response is null)
                return StatusCode(500, "Search Error");

            return Ok(response);
        }
    }
    


}
