using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/profile")]
    public class ProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<ProfileController> _logger;
        public ProfileController(AppDbContext context, ILogger<ProfileController> logger, ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _context = context;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddProfile([FromBody] ProfileModel profile)
        {
            _logger.LogInformation("[AddProfile]: " + JsonConvert.SerializeObject(profile));
            if (profile == null)
            {
                return BadRequest();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                profile.Created = DateTime.UtcNow;
                profile.ApplyDate = DateTime.SpecifyKind(profile.ApplyDate, DateTimeKind.Utc);
                profile.FireDate = null;
                _context.Profiles.Add(profile);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL AddProfile Error");
                }
                await transaction.CommitAsync();
            } catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[AddProfile]: " + e.ToString());
                return StatusCode(500);
            }
            
            
            var elasticProfile = profile.ToElasticProfileModel();

            var indexResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            profile.isIndexed = true;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return Ok(profile);
        }

        [HttpPost("add-adaccount")]
        public async Task<IActionResult> AddADAccount([FromBody] ADAccountModel account)
        {
            if (account == null)
                return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.ADAccounts.Add(account);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL AddAD Error");
                }
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[AddAD]: " + e.ToString());
                return StatusCode(500);
            }

            var profile = await _context.Profiles
                .Include(p => p.ADAccounts)
                .FirstOrDefaultAsync(p => p.Id == account.ProfileModelId);

            if (profile == null)
                return StatusCode(500, "Elastic profile error");

            var elasticProfile = profile.ToElasticProfileModel();
            _logger.LogInformation("[AddAD] Elastic model: " + JObject.FromObject(elasticProfile));
            var updateResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(account.ProfileModelId)
            );

            if (!updateResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch not updated");

            return Ok(updateResponse);
        }


        [HttpPost("update")]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileModel profile)
        {
            if (profile == null)
            {
                return BadRequest();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                profile.Created = DateTime.UtcNow;
                profile.ApplyDate = DateTime.SpecifyKind(profile.ApplyDate, DateTimeKind.Utc);
                if (profile.FireDate != null)
                {
                    profile.FireDate = DateTime.SpecifyKind(profile.FireDate.Value, DateTimeKind.Utc);
                }
                _context.Profiles.Update(profile);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL UpdProfile Error");
                }
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[UpdProfile]: " + e.ToString());
                return StatusCode(500);
            }

            var profileFromDb = await _context.Profiles
                .Include(p => p.ADAccounts)
                .FirstOrDefaultAsync(p => p.Id == profile.Id);
            if (profileFromDb == null)
                return StatusCode(500, "db error");


            var elasticProfile = profileFromDb.ToElasticProfileModel();


            var indexResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            profile.isIndexed = true;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();


            return Ok(indexResponse);
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProfile([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                return BadRequest("Invalid id");

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == guid);
            if (profile == null)
                return NotFound("Profile not found");

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            var response = await _elasticsearchClient.DeleteAsync<ElasticProfileModel>(guid, d => d.Index("profiles"));

            if (!response.IsValidResponse)
                return StatusCode(500, "Elasticsearch delete failed");

            return Ok("Profile deleted");
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            var profiles = await _context.Profiles.ToListAsync();
            foreach (var profile in profiles)
            {
                var elasticProfile = profile.ToElasticProfileModel();
                var response = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                    .Index("profiles")
                    .Id(profile.Id));

                if (response.IsValidResponse)
                {
                    profile.isIndexed = true;
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Resync complete");
        }
    }
}
