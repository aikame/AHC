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
    public class domain
    {
        public string name = "192.168.64.148:7096";
    }
    public class UserInfoRequest
    {
        public string User { get; set; }
        public string Domain { get; set; }
    }
    public class ProfileModel
    {

        public string name { get; set; }

        public string surname { get; set; }

        public string patronymic { get; set; }

        public string email { get; set; }

        public string company { get; set; }

        public string apply_date { get; set; }

        public string appointment { get; set; }

        public string city { get; set; }

        public string department { get; set; }

        public List<string> profiles { get; set; } = new List<string>();
        public string img_src { get; set; } = ".";
    }
}
