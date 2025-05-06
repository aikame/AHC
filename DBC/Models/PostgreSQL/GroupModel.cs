using DBC.Models.Elastic;
using DBC.Models.Shared;

namespace DBC.Models.PostgreSQL
{
    public class GroupModel
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
        public Guid DomainId { get; set; }
        public DomainModel Domain { get; set; }
        public bool isIndexed { get; set; } = false;

        public ElasticGroupModel ToElastic() {
            return new ElasticGroupModel
            {
                Id = this.Id,
                Description = this.Description,
                DistinguishedName = this.DistinguishedName,
                GroupCategory = this.GroupCategory,
                GroupScope = this.GroupScope,
                Name = this.Name,
                ObjectClass = this.ObjectClass,
                ObjectGUID = this.ObjectGUID,
                SamAccountName = this.SamAccountName,
                SID = this.SID,
                DomainName = this.Domain.Forest
            };

        }
    }
}
