using Backend.Models.Data;

namespace Backend.Models.Requests.Account
{
    public class AuthenticationRequest
    {
        public required string user { get; set; } 
        public required string password { get; set; }
    }
}
