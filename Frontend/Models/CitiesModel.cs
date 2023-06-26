using System.ComponentModel.DataAnnotations;

namespace Frontend.Models
{
    public class CitiesModel
    {
        [Key]
        public int Id { get; set; }
        public string City { get; set; }
        public string CityRus { get; set; }
    }
}
