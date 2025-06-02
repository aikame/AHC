using DBC.Data;
using DBC.Models.Elastic;
using DBC.Models.PostgreSQL;
using Elastic.Clients.Elasticsearch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using Elastic.Clients.Elasticsearch.Core.Search;
using static System.Runtime.InteropServices.JavaScript.JSType;
using DBC.Interfaces;

namespace DBC.Controllers
{
    [ApiController]
    [Route("/computer")]
    public class ComputerController : ControllerBase
    {
        private readonly IComputerService _computerService;
        private readonly ILogger<ComputerController> _logger;
        public ComputerController(ILogger<ComputerController> logger, IComputerService computerService)
        {
            _computerService = computerService;
            _logger = logger;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddComputer([FromBody] ComputerModel computer)
        {
            _logger.LogInformation("[AddComputer]: " + JsonConvert.SerializeObject(computer));
            if (computer == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _computerService.AddComputer(computer);
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
                _logger.LogError("[AddComputer]: " + e.Message);
                return Problem(e.Message);
            }
       
        }
        [HttpPost("update")]
        public async Task<IActionResult> UpdateComputer([FromBody] ComputerModel computer)
        {
            _logger.LogInformation("[UpdateComputer]: " + JsonConvert.SerializeObject(computer));
            if (computer == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _computerService.UpdateComputer(computer);
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
                _logger.LogError("[UpdateComputer]: " + e.Message);
                return Problem(e.Message);
            }
     
        }
        
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteComputer([FromQuery] string id)
        {
            _logger.LogInformation("[DeleteComputer]: " + JsonConvert.SerializeObject(id));
            if (id == null)
            {
                return BadRequest();
            }
            try
            {
                var result = await _computerService.DeleteComputer(id);
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[DeleteComputer]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpPost("reindexate")]
        public async Task<IActionResult> Reindexate()
        {
            _logger.LogInformation("[reindexate]: ");
            try
            {
                var result = await _computerService.Reindexate();
                if (result)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception e)
            {
                _logger.LogError("[reindexate]: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}

