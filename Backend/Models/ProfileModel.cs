using Newtonsoft.Json.Linq;

namespace Backend.Models
{
    public class ProfileModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string? Email { get; set; } = " ";

        public string? Company { get; set; }

        public string? ApplyDate { get; set; }
        public string? FireDate { get; set; } = null;
        public string? Appointment { get; set; }

        public string? City { get; set; }

        public bool ADreq { get; set; }

        public List<JObject> Profiles { get; set; } = new List<JObject>();
        public string ImgSrc { get; set; } = ".";
    }
}
