namespace DBC.Models.PostgreSQL
{
    public class ADAccountModel
    {
        public Guid Id { get; set; }
        public string? SID { get; set; }
        public string? SamAccountName { get; set; }
        public string? Domain { get; set; }

        public Guid ProfileModelId { get; set; }
        public ProfileModel Profile { get; set; }

    }
}
