namespace DBC.Models.PostgreSQL
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public DateTime created { get; } = DateTime.Now;
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

        //public List<string> profiles { get; set; } = new List<string>();
        public string img_src { get; set; } = ".";
    }
}
