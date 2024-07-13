using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace _1CC.Controllers
{
    [ApiController]
    [Route("/1connector/hs/post/")]
    public class OneCController : Controller
    {
        public class CustomHttpClientHandler : HttpClientHandler
        {
            public CustomHttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = ValidateServerCertificate;
            }

            private bool ValidateServerCertificate(HttpRequestMessage message, X509Certificate2 cert, X509Chain chain, SslPolicyErrors errors)
            {
                if (errors == SslPolicyErrors.None)
                {
                    return true;
                }

                // Для разработки, игнорируем ошибки сертификата
                return true; // !!! Не используйте в Production
            }
        }

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
            var sdata = JsonConvert.SerializeObject(RecData);
            using (HttpClient client = new HttpClient(new CustomHttpClientHandler()))
            {
                var jsonContent = new StringContent(sdata, Encoding.UTF8, "application/json");
                var result = await client.PostAsync("https://localhost:7095/CreateProfile", jsonContent);

                var responseContent = await result.Content.ReadAsStringAsync();
                Console.WriteLine(responseContent);

                if (result.IsSuccessStatusCode)
                {
                    return Content(responseContent);
                }
                else
                {
                    return BadRequest("Произошла ошибка при выполнении запроса.");
                }
            }
            var sendData = JsonConvert.SerializeObject(RecData);
            return Ok();
        }

        
    }
    
}