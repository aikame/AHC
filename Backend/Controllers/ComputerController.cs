using Backend.Interfaces;
using Backend.Models.Data;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;

namespace Backend.Controllers
{
    [ApiController]
    [Route("/")]
    public class ComputerController : ControllerBase
    {
        private readonly string _connectorPort;
        private readonly HttpClient _client;
        private readonly ILogger<ComputerController> _logger;
        private readonly IComputerService _computerService;
        public ComputerController(ILogger<ComputerController> logger, IComputerService computerService, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _client = httpClientFactory.CreateClient("GroupController");
            _logger = logger;
            _connectorPort = configuration["connectorPort"];
            _computerService = computerService;
        }

        [HttpPost("CollectComputerInfo")]
        public async Task<IActionResult> CollectComputerInfo([FromBody] ComputerModel computer)
        {
            try
            {
                _logger.LogInformation("[CollectComputerInfo]: " + System.Text.Json.JsonSerializer.Serialize(computer));
                var result = await _computerService.CollectComputerInfo(computer);
                return result ? Ok() : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[CollectComputerInfo]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("CheckComputer")]
        public async Task<IActionResult> CheckComputer([FromQuery] string _id)
        {
            try
            {
                if (!Guid.TryParse(_id, out var guid))
                    return BadRequest("Invalid GUID");
                var computer = new ComputerModel { Id = guid };
                var result = await _computerService.CheckComputer(computer);
                return result ? Ok() : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckComputer]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("GetAppInfo")]
        public async Task<IActionResult> GetAppInfo([FromQuery] string computer, [FromQuery] string domain)
        {
            try
            {
                var computerModel = new ComputerModel { ComputerName = computer };
                var result = await _computerService.GetAppInfo(computerModel);

                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckComputer]: " + e.Message);
                return Problem(e.Message);
            }
        }

        [HttpGet("domainList")]
        public async Task<IActionResult> GetDomainList()
        {
            try
            {
                var domainList = await _computerService.GetDomainList();
                return domainList is not null ? Content(JsonConvert.SerializeObject(domainList)) : BadRequest("Error");
            }
            catch (Exception e)
            {
                _logger.LogError("[CheckComputer]: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}
