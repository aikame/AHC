using Backend.Models.Data;

namespace Backend.Models.Requests.Group
{
    public class AddToGroupRequest
    {
        public required ADAccountModel user { get; set; }
        public required GroupModel group { get; set; }
    }
}
