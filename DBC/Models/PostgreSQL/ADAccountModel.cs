namespace DBC.Models.PostgreSQL
{
    public class ADAccountModel
    {
        required public string SID { get; set; }
        required public string SamAccountName { get; set; }
        required public string Domain { get; set; }

        required public string ProfileID { get; set; }

    }
}
