using DBC.Data;
using DBC.Interfaces;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace DBC.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<AccountService> _logger;
        public AccountService(AppDbContext context, ILogger<AccountService> logger, ElasticsearchClient elasticsearchClient)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
            _context = context;
        }

        public async Task<ElasticProfileModel?> AddADAccount(ADAccountModel account)
        {
            if (account == null)
                return null;
            var existingDomain = await _context.Domains
            .FirstOrDefaultAsync(d => d.Forest == account.Domain.Forest);
            if (existingDomain == null)
                return null;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                account.Domain = existingDomain;
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
                return  null;
            }

            var profile = await _context.Profiles
                .Include(p => p.ADAccounts)
                .FirstOrDefaultAsync(p => p.Id == account.ProfileId);

            if (profile == null)
                return null;

            var elasticProfile = profile.ToElasticProfileModel();
            _logger.LogInformation("[AddAD] Elastic model: " + JObject.FromObject(elasticProfile));
            var updateResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(account.ProfileId)
            );

            if (!updateResponse.IsValidResponse)
                return null;

            return elasticProfile;
        }

        public async Task<ElasticProfileModel?> UpdateADAccount(ADAccountModel account)
        {
            if (account == null)
                return null;
            var existingAcc = await _context.ADAccounts
            .Include(a => a.Domain)
            .FirstOrDefaultAsync(a => a.DistinguishedName == account.DistinguishedName);
            if (existingAcc == null)
                return null;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                existingAcc.Enabled = account.Enabled;

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
                return null;
            }

            var profile = await _context.Profiles
                .Include(p => p.ADAccounts)
                .FirstOrDefaultAsync(p => p.Id == existingAcc.ProfileId);

            if (profile == null)
                return null;

            var elasticProfile = profile.ToElasticProfileModel();
            _logger.LogInformation("[AddAD] Elastic model: " + JObject.FromObject(elasticProfile));
            var updateResponse = await _elasticsearchClient.IndexAsync(elasticProfile, i => i
                .Index("profiles")
                .Id(profile.Id)
            );

            if (!updateResponse.IsValidResponse)
                return null;

            return elasticProfile;
        }
    }
}
