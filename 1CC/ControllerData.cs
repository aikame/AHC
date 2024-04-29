using System.Text.RegularExpressions;

namespace _1CC
{

    public class ReceivedData
    {
        private int MinLenght = 2;

        public string Name = ""; 

        public string Surname = "";

        public string Patronymic = "";

        public string Email = "";

        public string Company = "";

        public string Apply_Date = "";

        public string Appointment = "";

        public string City = "";

        public bool CheckForTroubles()
        {
            if (Name.Length < MinLenght || Surname.Length < MinLenght || Email.Length < MinLenght ||
                Company.Length < MinLenght || Apply_Date.Length < MinLenght || Appointment.Length < MinLenght || City.Length < MinLenght)
                return false;

            Regex regex = new Regex(@"^[à-ÿÀ-ß]+$");
            if (!(regex.IsMatch(Name) && regex.IsMatch(Surname) && regex.IsMatch(Appointment) && regex.IsMatch(City) && (regex.IsMatch(Patronymic) || Patronymic=="")))
            {
                Console.WriteLine("Illegal characters detected");
                return false;
            }

            regex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");

            if (regex.IsMatch(Email))
            {
                Console.WriteLine("The email address is valid.");
            }
            else
            {
                Console.WriteLine("The email address is invalid.");
                return false;
            }

            try
            {
                DateOnly donly = DateOnly.Parse(Apply_Date);
                Console.WriteLine(donly.ToString());
            }
            catch 
            {
                Console.WriteLine("Date parse error");
                return false;
            }
            

            return true;
        }

    }


}