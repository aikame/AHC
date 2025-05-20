using ADDC.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Speech.Synthesis;

namespace ADDC.Controllers
{
    [ApiController]
    [Route("/")]
    public class ComputerController : Controller
    {
        private readonly ILogger<ComputerController> _logger;
        private readonly IComputerService _computerService;
        public ComputerController(ILogger<ComputerController> logger, IComputerService service)
        {
            _computerService = service;
            _logger = logger;
        }


        [HttpGet("GetAppInfo")]
        public async Task<IActionResult> GetAppInfo()
        {
            try
            {
                var result = await _computerService.GetAppInfo();
                return result is not null ? Ok(result) : BadRequest();
            }
            catch (Exception e) {
                return Problem(e.Message);
            }
        }
        [HttpGet("GetComputerInfo")]
        public async Task<IActionResult> GetComputerInfo()
        {
            _logger.LogInformation($"[GetComputerInfo]");
            var result = await _computerService.GetInfo();
            try
            {
                return result is not null ? Content(JsonConvert.SerializeObject(result)) : BadRequest() ;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetComputerInfo] JSON Parse Error: " + e.Message);
                return Problem(e.Message);
            }
        }
    }
}
