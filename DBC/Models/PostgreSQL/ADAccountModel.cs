using DBC.Models.Elastic;
using DBC.Models.Shared;

namespace DBC.Models.PostgreSQL
{
    public class ADAccountModel
    {
        public Guid Id { get; set; }
        public string? ObjectGUID { get; set; }
        public string? SamAccountName { get; set; }
        public string? DistinguishedName { get; set; }
        public Guid DomainId { get; set; }
        public DomainModel Domain { get; set; }

        public Guid ProfileModelId { get; set; }
        public ProfileModel? Profile { get; set; }

        public bool? Enabled { get; set; }
        public ElasticADAccountModel ToElastic()
        {
            return new ElasticADAccountModel
            {
                Id = this.Id,
                ObjectGUID = this.ObjectGUID,
                SamAccountName = this.SamAccountName,
                DistinguishedName = this.DistinguishedName,
                Domain = this.Domain.Forest,
                Enabled = this.Enabled
            };
        }
    }
}
