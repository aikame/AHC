namespace DBC.Models.Elastic
{
    public class ElasticGroupModel
    {
        public Guid Id { get; set; }
        public string? Description { get; set; }
        public string? DistinguishedName { get; set; }
        public string? GroupCategory { get; set; }
        public string? GroupScope { get; set; }
        public string? Name { get; set; }
        public string? ObjectClass { get; set; }
        public string? ObjectGUID { get; set; }
        public string? SamAccountName { get; set; }
        public string? SID { get; set; }
        public string? DomainName { get; set; }

    }
}
