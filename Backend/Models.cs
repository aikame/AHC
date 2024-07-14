using System.Text.Json;

namespace Backend.models
{
    public class UserModel
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Patronymic { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string Company { get; set; }
        public string Apply_date { get; set; }
        public string Appointment { get; set; }
        public string Department { get; set;}
    }

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

        //public string department { get; set; }

        public List<string> profiles { get; set; } = new List<string>();
        public string img_src { get; set; } = ".";
    }
    public class ComputerModel
    {
        public string WindowsEdition { get; set; }
        public string IPAddress { get; set; }
        public string DomainName { get; set; }
        public float TotalRAMGB { get; set; }
        public List<JsonElement> DiskSpace  { get; set; }
        public List<string> CPUName { get; set; }
        public List<int> CPUCores { get; set; }
        public string ComputerName { get; set; }
        public bool Status { get; set; } = true;

    }
}
