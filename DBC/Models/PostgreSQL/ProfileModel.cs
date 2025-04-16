namespace DBC.Models.PostgreSQL
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; } = DateTime.Now;
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string  Email { get; set; } = " ";

        public string? Company { get; set; }

        public string? ApplyDate { get; set; }
        public string FireDate { get; set; } = "";
        public string? Appointment { get; set; }

        public string? City { get; set; }

        public bool ADreq { get; set; }
        public string ImgSrc { get; set; } = ".";
    }
}
