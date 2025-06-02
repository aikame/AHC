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
    public class GroupService : IGroupService
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<GroupService> _logger;
        public GroupService(AppDbContext context, ILogger<GroupService> logger, ElasticsearchClient elasticsearchClient)
        {
            _logger = logger;
            _elasticsearchClient = elasticsearchClient;
            _context = context;
        }

        public async Task<ElasticGroupModel?> AddGroup(GroupModel group)
        {
            _logger.LogInformation("[AddGroup]: " + JsonConvert.SerializeObject(group));
            if (group == null)
            {
                return null;
            }
            var existingDomain = await _context.Domains
            .FirstOrDefaultAsync(d => d.Forest == group.Domain.Forest);
            if (existingDomain == null)
                return null;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                group.Domain = existingDomain;
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
                return null;
            }


            var indexResponse = await _elasticsearchClient.IndexAsync(group.ToElastic(), i => i
                .Index("groups")
                .Id(group.Id)
            );

            if (!indexResponse.IsValidResponse)
                return null;

            group.isIndexed = true;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();

            return group.ToElastic();
        }

        public async Task<ElasticGroupModel?> UpdateGroup( GroupModel group)
        {
            if (group == null)
            {
                return null;
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
                return null;
            }


            var indexResponse = await _elasticsearchClient.IndexAsync(group.ToElastic(), i => i
                .Index("groups")
                .Id(group.Id)
            );

            if (!indexResponse.IsValidResponse)
                return null;

            group.isIndexed = true;
            _context.Groups.Update(group);
            await _context.SaveChangesAsync();


            return group.ToElastic();
        }

        public async Task<bool> DeleteGroups(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                return false;

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == guid);
            if (group == null)
                return false;

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            var response = await _elasticsearchClient.DeleteAsync<GroupModel>(guid, d => d.Index("groups"));

            if (!response.IsValidResponse)
                return false;

            return true;
        }

        public async Task<bool> Reindexate()
        {
            var groups = await _context.Groups.Include(g => g.Domain).ToListAsync();
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
            return true;
        }
    }
}
