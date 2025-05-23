﻿using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Elastic.Clients.Elasticsearch.Core.Search;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/computer")]
    public class ComputerController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ElasticsearchClient _elasticsearchClient;
        private readonly ILogger<ComputerController> _logger;
        public ComputerController(AppDbContext context, ILogger<ComputerController> logger, ElasticsearchClient elasticsearchClient)
        {
            _elasticsearchClient = elasticsearchClient;
            _context = context;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddComputer([FromBody] ComputerModel computer)
        {
            _logger.LogInformation("[AddComputer]: " + JsonConvert.SerializeObject(computer));
            if (computer == null)
                return BadRequest();

            using var transaction = await _context.Database.BeginTransactionAsync();
            bool isNewDomain = false;
            try
            {
                var existingDomain = await _context.Domains
            .FirstOrDefaultAsync(d => d.Forest == computer.Domain.Forest);

                if (existingDomain == null)
                {
                    existingDomain = computer.Domain;
                    existingDomain.Id = Guid.NewGuid();
                    _context.Domains.Add(existingDomain);
                    await _context.SaveChangesAsync();
                    isNewDomain = true;
                }
                computer.DomainId = existingDomain.Id;
                computer.Domain = null;
                var existing = await _context.Computers
                    .AsTracking()
                    .FirstOrDefaultAsync(p => p.ComputerName == computer.ComputerName);

                if (existing != null)
                {
                    // Обновляем вручную все поля
                    existing.Updated = DateTime.UtcNow;
                    existing.WindowsEdition = computer.WindowsEdition;
                    existing.IPAddress = computer.IPAddress;
                    existing.DomainId = computer.DomainId;
                    existing.TotalRAMGB = computer.TotalRAMGB;
                    existing.DiskSpace = computer.DiskSpace;
                    existing.CPUName = computer.CPUName;
                    existing.CPUCores = computer.CPUCores;
                    existing.ComputerRole = computer.ComputerRole;
                    existing.Status = computer.Status;
                    existing.isIndexed = false;
                }
                else
                {
                    computer.Id = Guid.NewGuid(); // если не задан
                    _context.Computers.Add(computer);
                }

                var status = await _context.SaveChangesAsync();
                if (status == 0)
                    throw new Exception("SQL AddComputer Error");

                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                isNewDomain = false;
                _logger.LogError("[AddComputer]: " + e);
                return StatusCode(500);
            }
            
            var compToIndex = await _context.Computers.FirstOrDefaultAsync(p => p.ComputerName == computer.ComputerName);
            IndexResponse domainIndexResponse;
            var isValid = true;
            if (isNewDomain)
            {
                domainIndexResponse = await _elasticsearchClient.IndexAsync(compToIndex.Domain, i => i
                .Index("domains")
                .Id(compToIndex.Domain.Id)
                );
                isValid = domainIndexResponse.IsValidResponse;
            }
            var elasticComp = compToIndex.ToElastic();
            var indexResponse = await _elasticsearchClient.IndexAsync(elasticComp, i => i
                .Index("computers")
                .Id(elasticComp.Id)
            );

            if (!indexResponse.IsValidResponse || isNewDomain ? !isValid : false)
                return StatusCode(500, "ElasticSearch AddComputer error");

            // Обновляем флаг
            compToIndex.isIndexed = true;
            await _context.SaveChangesAsync();

            return Ok(indexResponse);
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateComputer([FromBody] ComputerModel computer)
        {
            if (computer == null)
            {
                return BadRequest();
            }
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingDomain = await _context.Domains
.FirstOrDefaultAsync(d => d.Forest == computer.Domain.Forest);

                if (existingDomain == null)
                {
                    return BadRequest("invalid domain");
                }
                computer.Domain = existingDomain;
                computer.Updated = DateTime.UtcNow;
                _context.Computers.Update(computer);
                var status = await _context.SaveChangesAsync();
                if (status == 0)
                {
                    throw new Exception("SQL UpdateComputer Error");
                }
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError("[UpdateComputer]: " + e.ToString());
                return StatusCode(500);
            }


            var indexResponse = await _elasticsearchClient.IndexAsync(computer.ToElastic(), i => i
                .Index("computers")
                .Id(computer.Id)
            );

            if (!indexResponse.IsValidResponse)
                return StatusCode(500, "ElasticSearch error");

            computer.isIndexed = true;
            _context.Computers.Update(computer);
            await _context.SaveChangesAsync();


            return Ok(indexResponse);
        }
        
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteComputer([FromQuery] string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var guid))
                return BadRequest("Invalid id");

            var computer = await _context.Computers.FirstOrDefaultAsync(c => c.Id == guid);
            if (computer == null)
                return NotFound("Computer not found");

            _context.Computers.Remove(computer);
            await _context.SaveChangesAsync();

            var response = await _elasticsearchClient.DeleteAsync<ComputerModel>(guid, d => d.Index("computers"));

            if (!response.IsValidResponse)
                return StatusCode(500, "Elasticsearch delete failed");

            return Ok("Computer deleted");
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            var computers = await _context.Computers.Include(c => c.Domain).ToListAsync();
            foreach (var computer in computers)
            {
                var response = await _elasticsearchClient.IndexAsync(computer.ToElastic(), i => i
                    .Index("computers")
                    .Id(computer.Id));

                if (response.IsValidResponse)
                {
                    computer.isIndexed = true;
                }
            }
            await _context.SaveChangesAsync();
            return Ok("Resync complete");
        }
    }
}

