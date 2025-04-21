namespace DBC.Models.PostgreSQL
{
    public class ProfileModel
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } 
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string  Email { get; set; } = " ";

        public string? Company { get; set; }

        public DateTime ApplyDate { get; set; }
        public DateTime? FireDate { get; set; }
        public string? Appointment { get; set; }

        public string? City { get; set; }
        public string ImgSrc { get; set; } = ".";
    }
}
