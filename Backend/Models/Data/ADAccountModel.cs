namespace Backend.Models.Data
{
    public class ADAccountModel
    {
        public string? DistinguishedName { get; set; }
        public string? SamAccountName { get; set; }
        public string? EmailAddress { get; set; }
        public bool? Enabled { get; set; } = false;
        public bool? PasswordExpired { get; set; } = false;
        public DateTime? PasswordLastSet { get; set; } = DateTime.UtcNow;
        public List<string>? MemberOf { get; set; }
        public DomainModel? Domain { get; set; }

        public string? ProfileId { get; set; }
    }
}
