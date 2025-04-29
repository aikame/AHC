using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace Backend.models
{
    public class ProfileModel
    {

        public string Name { get; set; }

        public string Surname { get; set; }

        public string Patronymic { get; set; }

        public string Email { get; set; } = " ";

        public string Company { get; set; }

        public string ApplyDate { get; set; }
        public string? FireDate { get; set; } = null;
        public string Appointment { get; set; }

        public string City { get; set; }

        public bool ADreq { get; set; }

        public List<string> Profiles { get; set; } = new List<string>();
        public string ImgSrc { get; set; } = ".";
    }
    public class ComputerModel
    {
        public int? Id { get; set; }
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public JsonElement Domain { get; set; }
        public float TotalRAMGB { get; set; }
        public List<JsonElement> DiskSpace  { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public int ComputerRole { get; set; }
        public bool Status { get; set; } = true;

    }
}
