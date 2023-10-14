namespace Backend
{
    public class UserModel
    {
        public string name, RUname,
               surname, RUsurname,
               midname, RUmidname,
               city, RUcity,
               company, RUcompany, 
               department, RUdepartment,
               appointment, RUappointment;
    }
    public class UserInfo {
        public string DistinguishedName, SamAccountName, EmailAddress, PasswordLastSet, MemberOf;
        public bool Enabled, PasswordExpired;
    }
}
