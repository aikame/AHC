namespace ADDC.Models.Data
{
    //using System.ComponentModel.DataAnnotations;

    public class ProfileModel
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? SurName { get; set; }
        public string? Patronymic { get; set; }
        public string? City { get; set; }
        public string? Company { get; set; }
        public string? Department { get; set; }
        public string? Appointment { get; set; }
    }


}
