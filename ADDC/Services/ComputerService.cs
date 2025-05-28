using ADDC.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using ADDC.Models.Data;

namespace ADDC.Services
{
    public class ComputerService : IComputerService
    {
        private readonly IPowershellSessionPoolService _sessionPool;
        private readonly ILogger<ComputerService> _logger;
        public ComputerService(IPowershellSessionPoolService sessionPool,ILogger<ComputerService> logger)
        {
            _sessionPool = sessionPool;
            _logger = logger;
        }
        public async Task<JObject?> GetAppInfo()
        {
            _logger.LogInformation($"GetAppInfo");
            var result = await _sessionPool.ExecuteFunction("GetAppInfo");
            try
            {
                JObject jsonData = JObject.Parse(result);

                return jsonData;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetAppInfo] JSON Parse Error: " + e.Message);
                return null;
            }
        }
        public async Task<ComputerModel?> GetInfo()
        {
            _logger.LogInformation($"[GetComputerInfo]");
            var func = _sessionPool.ExecuteFunction("CollectInfo");
            string result = func.Result;
            try
            {
                JObject jsonData = JObject.Parse(result);
                ComputerModel computer = jsonData.ToObject<ComputerModel>();

                return computer;
            }
            catch (Exception e)
            {
                _logger.LogError("[GetComputerInfo] JSON Parse Error: " + e.Message);
                return null;
            }

        }
    }
}
