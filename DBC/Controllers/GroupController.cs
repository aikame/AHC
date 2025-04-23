using DBC.Data;
using DBC.Models.Shared;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/group")]
    public class GroupController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<GroupController> _logger;
        public GroupController(AppDbContext context, ILogger<GroupController> logger, ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _context = context;
            _logger = logger;
        }
        [HttpPost("add")]
        public async Task<IActionResult> AddGroup([FromBody] GroupModel group)
        {
            _logger.LogInformation("[AddGroup]: " + JsonConvert.SerializeObject(group));
            if (group == null)
            {
                return BadRequest();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Groups.Add(group);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL AddGroup Error");
                }
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[AddGroup]: " + e.ToString());
                return StatusCode(500);
            }


            var indexResponse = await _elasticsearchClient.IndexAsync(group, i => i
                .Index("groups")
                .Id(group.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            group.isIndexed = true;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            return Ok(indexResponse);
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateGroup([FromBody] GroupModel group)
        {
            if (group == null)
            {
                return BadRequest();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Groups.Update(group);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL UpdateGroup Error");
                }
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[UpdateGroup]: " + e.ToString());
                return StatusCode(500);
            }


            var indexResponse = await _elasticsearchClient.IndexAsync(group, i => i
                .Index("groups")
                .Id(group.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            group.isIndexed = true;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();


            return Ok(indexResponse);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteGroups([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                return BadRequest("Invalid id");

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == guid);
            if (group == null)
                return NotFound("group not found");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            var response = await _elasticsearchClient.DeleteAsync<GroupModel>(guid, d => d.Index("groups"));

            if (!response.IsValidResponse)
                return StatusCode(500, "Elasticsearch delete failed");

            return Ok("group deleted");
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            var groups = await _context.Groups.ToListAsync();
            foreach (var group in groups)
            {
                var response = await _elasticsearchClient.IndexAsync(group, i => i
                    .Index("groups")
                    .Id(group.Id));

                if (response.IsValidResponse)
                {
                    group.isIndexed = true;
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Resync complete");
        }
    }
}
