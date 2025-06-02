using DBC.Data;
using DBC.Interfaces;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace DBC.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<ProfileService> _logger;
        public ProfileService(AppDbContext context, ILogger<ProfileService> logger, ElasticsearchClient elasticsearchClient)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
            _context = context;
        }

        public async Task<ElasticProfileModel?> AddProfile(ProfileModel profile)
        {
            _logger.LogInformation("[AddProfile]: " + JsonConvert.SerializeObject(profile));
            if (profile == null)
            {
                return null;
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
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[AddProfile]: " + e.ToString());
                return null;
            }


            var elasticProfile = profile.ToElasticProfileModel();

            var indexResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!indexResponse.IsValidResponse)
                return null;

            profile.isIndexed = true;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();
            return elasticProfile;
        }


        public async Task<ElasticProfileModel?> UpdateProfile( ProfileModel profile)
        {
            if (profile == null)
            {
                return null;
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
                return null;
            }

            var profileFromDb = await _context.Profiles
                .Include(p => p.ADAccounts)
                .ThenInclude(a => a.Domain)
                .FirstOrDefaultAsync(p => p.Id == profile.Id);

            if (profileFromDb == null)
                return null;


            var elasticProfile = profileFromDb.ToElasticProfileModel();


            var indexResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!indexResponse.IsValidResponse)
                return null;

            profile.isIndexed = true;
            _context.Profiles.Update(profile);
            await _context.SaveChangesAsync();


            return elasticProfile;
        }

        public async Task<bool> DeleteProfile( string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                return false;

            var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.Id == guid);
            if (profile == null)
                return false;

            _context.Profiles.Remove(profile);
            await _context.SaveChangesAsync();

            var response = await _elasticsearchClient.DeleteAsync<ElasticProfileModel>(guid, d => d.Index("profiles"));

            if (!response.IsValidResponse)
                return false;

            return true;
        }

        public async Task<bool> Reindexate()
        {
            var profiles = await _context.Profiles.Include(p => p.ADAccounts).ThenInclude(p => p.Domain).ToListAsync();
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
            return true;
        }
    }
}
