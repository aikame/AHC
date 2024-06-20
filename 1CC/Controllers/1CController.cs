using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _1CC.Controllers
{
    [ApiController]
    [Route("/1connector/hs/post/")]
    public class OneCController : Controller
    {

        [HttpPost("read1cjson")]
        public async Task<ActionResult> Rec1CData([FromBody] JObject data)
        {
            Console.WriteLine("New Request");
            Console.WriteLine(data);
            var RecData = data.ToObject<ReceivedData>();
            if (!RecData.CheckForTroubles())
            {
                Console.WriteLine("Recieved bad data");
                return BadRequest();
            }
            else
            {
                Console.WriteLine("Recieved data is ok");
            }
            var sendData = JsonConvert.SerializeObject(RecData);
            return Ok();
        }

        
    }
    
}