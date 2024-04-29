using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IISIntegration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace _1CC.Controllers
{
    [ApiController]
    [Route("/")]
    public class OneCController : ControllerBase
    {

        [HttpPost(Name = "1CData")]
        public async Task<ActionResult> Rec1CData([FromBody] JObject data)
        {
            Console.WriteLine("New Request");
            var RecData = data.ToObject<ReceivedData>();
            Console.WriteLine(RecData.ToString());
            if (!RecData.CheckForTroubles())
            {
                Console.WriteLine("Recieved bad data");
                return BadRequest();
            }
            SendData sendData = new SendData();
            sendData.Name = RecData.Name.ToCharArray();
            sendData.Surname = RecData.Surname.ToCharArray();
            sendData.Patronymic = RecData.Patronymic.ToCharArray();
            sendData.Email = RecData.Email.ToCharArray();
            sendData.Company = RecData.Company.ToCharArray();
           
            sendData.ApplyDate = DateOnly.Parse(RecData.ApplyDate);

            sendData.Appointment = RecData.Appointment.ToCharArray();
            sendData.City = RecData.City.ToCharArray();
            Console.WriteLine(sendData.ToString());
            return Ok();
        }
       

    }
    
}