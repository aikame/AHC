namespace DBC.Models.Elastic
{
    public class ElasticADAccountModel
    {
        public Guid Id { get; set; }
        public string? ObjectGUID { get; set; }
        public string? SamAccountName { get; set; }
        public string? DistinguishedName { get; set; }
        public string? Domain { get; set; }
    }

}
