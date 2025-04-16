using System.Text.Json;

namespace Backend.models
{
    public class ProfileModel
    {

        public string name { get; set; }

        public string surname { get; set; }

        public string patronymic { get; set; }

        public string email { get; set; } = " ";

        public string company { get; set; }

        public string apply_date { get; set; }
        public string fire_date { get; set; } = "";
        public string appointment { get; set; }

        public string city { get; set; }

        public bool ADreq { get; set; }

        public List<string> profiles { get; set; } = new List<string>();
        public string img_src { get; set; } = ".";
    }
    public class ComputerModel
    {
        public int? _Id { get; set; }
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public string DomainName { get; set; }
        public float TotalRAMGB { get; set; }
        public List<JsonElement> DiskSpace  { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public int ComputerRole { get; set; }
        public bool Status { get; set; } = true;

    }
}
