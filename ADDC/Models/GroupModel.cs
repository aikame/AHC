namespace ADDC.Models
{
    public class GroupModel
    {
        public string? Description { get; set; }
        public string? DistinguishedName { get; set; }
        public string? GroupCategory { get; set; }
        public string? GroupScope { get; set; }
        public string? Name { get; set; }
        public string? ObjectClass { get; set; }
        public string? ObjectGUID { get; set; }
        public string? SamAccountName { get; set; }
        public string? SID { get; set; }
        public Guid DomainId { get; set; }
        public bool isIndexed { get; set; } = false;

    }
}
