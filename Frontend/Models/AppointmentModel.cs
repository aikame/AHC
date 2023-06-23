using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class AppointmentModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string NameRus { get; set; }
    }
}
