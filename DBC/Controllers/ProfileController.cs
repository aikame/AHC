using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
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
        [HttpPut("add")]
        public async Task<IActionResult> AddProfile([FromBody] ProfileModel profile)
        {
            if (profile == null)
            {
                return BadRequest();
            }
            profile.Created = DateTime.UtcNow;
            profile.ApplyDate = DateTime.SpecifyKind(profile.ApplyDate, DateTimeKind.Utc);
            profile.FireDate = null;
            Console.WriteLine(JsonConvert.SerializeObject(profile));
            _context.Profiles.Add(profile);
            await _context.SaveChangesAsync();

            var elasticProfile = new ElasticProfileModel
            {
                Id = profile.Id,
                Created = profile.Created,
                Name = profile.Name ?? "",
                Surname = profile.Surname ?? "",
                Patronymic = profile.Patronymic ?? "",
                Email = profile.Email,
                Company = profile.Company ?? "",
                ApplyDate = profile.ApplyDate,
                FireDate = profile.FireDate,
                Appointment = profile.Appointment ?? "",
                City = profile.City ?? "",
                ImgSrc = profile.ImgSrc,
                Profiles = new List<JObject>()
            };

            var indexResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            return Ok(indexResponse);
        }
        
    }
}
