using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Linq;

namespace _1CC.Controllers
{
    [ApiController]
    [Route("/1connector/hs/post/")]
    public class OneCController : Controller
    {
        private readonly string _coreIP;
        public OneCController(IConfiguration configuration) {
            _coreIP = configuration["core"];
        }
        [HttpPost("read1cjson")]
        public async Task<ActionResult> Rec1CData([FromBody] JObject data)
        {
            Console.WriteLine("New Request");
            Console.WriteLine(data);
            var domain = data["domain"];
            Console.WriteLine(domain);
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
            var sdata = JsonConvert.SerializeObject(RecData);
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"https://{_coreIP}:7095/CreateProfile?domain="+domain+"&blocked=True", jsonContent);

                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("��������� ������ ��� ���������� �������.");
                }
            }
            var sendData = JsonConvert.SerializeObject(RecData);
            return Ok();
        }
        [HttpPost("fire")]
        public async Task<ActionResult> FireUser([FromBody] JObject data)
        {
            Console.WriteLine($"Fire: {data}");
            var sdata = JsonConvert.SerializeObject(data);
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"https://{_coreIP}:7095/FireUser", jsonContent);

                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("��������� ������ ��� ���������� �������.");
                }
            }

            return Ok();
        }

        [HttpPost("return")]
        public async Task<ActionResult> ReturnUser([FromBody] JObject data)
        {
            Console.WriteLine($"Return: {data}");
            var sdata = JsonConvert.SerializeObject(data);
            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata, Encoding.UTF8, "application/json");
                var result = await client.PostAsync($"https://{_coreIP}:7095/ReturnUser", jsonContent);

                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("��������� ������ ��� ���������� �������.");
                }
            }
            return Ok();
        }
        [HttpGet("domainList")]
        public async Task<ActionResult> GetDomainList([FromQuery] string? data)
        {

            using (HttpClient client = new HttpClient(new HttpClientHandler()))
            {
                var result = await client.GetAsync($"https://{_coreIP}:7095/domainList");

                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("��������� ������ ��� ���������� �������.");
                }
            }
            return Ok();
        }
    }
    
}