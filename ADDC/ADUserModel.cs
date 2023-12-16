namespace ADDC
{
    public class ADUserModel
    {
        public string DistinguishedName { get; set; }
        public string SamAccountName { get; set; }
        public string EmailAddress { get; set; }
        public bool Enabled { get; set; }
        public bool PasswordExpired { get; set; }
        public DateTime? PasswordLastSet { get; set; }
        public List<string> MemberOf { get; set; }
    }
}
