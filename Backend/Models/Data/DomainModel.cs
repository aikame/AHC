namespace Backend.Models.Data
{
    public class DomainModel
    {
        public Guid? Id { get; set; }
        public string Forest { get; set; } = "";
        public string Name { get; set; } = "";
        public string DomainSID { get; set; } = "";
    }
}
