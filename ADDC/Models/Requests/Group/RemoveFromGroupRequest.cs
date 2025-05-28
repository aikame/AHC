using ADDC.Models.Data;

namespace Backend.Models.Requests.Group
{
    public class RemoveFromGroupRequest
    {
        public required ADAccountModel user {  get; set; }
        public required GroupModel group { get; set; }
    }
}
