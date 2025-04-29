using System.Text.RegularExpressions;

namespace _1CC
{

    public class ReceivedData
    {
        private int MinLenght = 2;

        public string name = ""; 

        public string surname = "";

        public string patronymic = "";

        public string company = "";

        public string applydate = "";

        public string appointment = "";

        public string city = "";
        
        public bool ADreq = false;

        public bool CheckForTroubles()
        {
            if (name.Length < MinLenght || surname.Length < MinLenght || 
                company.Length < MinLenght || applydate.Length < MinLenght || appointment.Length < MinLenght || city.Length < MinLenght)
                return false;

            Regex regex = new Regex(@"^[à-ÿÀ-ß]+$");
            if (!(regex.IsMatch(name) && regex.IsMatch(surname) && regex.IsMatch(appointment) && regex.IsMatch(city) && (regex.IsMatch(patronymic) || patronymic=="")))
            {
                Console.WriteLine("Illegal characters detected");
                return false;
            }

            regex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");

            try
            {
                DateOnly donly = DateOnly.Parse(applydate);
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