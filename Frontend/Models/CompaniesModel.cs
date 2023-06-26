using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class CompaniesModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CityId { get; set; }
        public string Name { get; set; }
        public string NameRus { get; set; }
    }
}
