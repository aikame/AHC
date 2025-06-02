using DBC.Data;
using DBC.Interfaces;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/profile")]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly IAccountService _accountService;
        public AccountController(AppDbContext context, ILogger<AccountController> logger, IAccountService accountService, ElasticsearchClient elasticsearchClient)
        {
            _logger = logger;
            _accountService = accountService;
        }
        [HttpPost("add-adaccount")]
        public async Task<IActionResult> AddADAccount([FromBody] ADAccountModel account)
        {
            _logger.LogInformation("[add-adaccount]: " + JsonConvert.SerializeObject(account));
            if (account == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _accountService.AddADAccount(account);
                if (result is not null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[add-adaccount]: " + e.Message);
                return Problem(e.Message);
            }
            
        }

        [HttpPost("update-adaccount")]
        public async Task<IActionResult> UpdateADAccount([FromBody] ADAccountModel account)
        {
            _logger.LogInformation("[update-adaccount]: " + JsonConvert.SerializeObject(account));
            if (account == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _accountService.UpdateADAccount(account);
                if (result is not null)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[update-adaccount]: " + e.Message);
                return Problem(e.Message);
            }

        }
    }
}
