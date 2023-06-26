using System.ComponentModel.DataAnnotations;
namespace Frontend.Models
{
    public class DepartmentModel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string NameRus { get; set; }
    }
}
