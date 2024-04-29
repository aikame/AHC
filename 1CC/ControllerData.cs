namespace _1CC
{

    public class ReceivedData
    {
        private int MinLenght = 3;

        public string Name = ""; 

        public string Surname = "";

        public string Patronymic = "";

        public string Email = "";

        public string Company = "";

        public string ApplyDate = "";

        public string Appointment = "";

        public string City = "";

        public bool CheckForTroubles()
        {
            if (Name.Length < MinLenght || Surname.Length < MinLenght || Email.Length < MinLenght ||
                Company.Length < MinLenght || ApplyDate.Length < MinLenght || Appointment.Length < MinLenght || City.Length < MinLenght)
                return false;
            try
            {
                DateOnly donly = DateOnly.Parse(ApplyDate);
            }
            catch 
            {
                Console.WriteLine("Date parse error");
                return false;
            }
            

            return true;
        }

    }

    public class SendData
    {
        public char[] Name = new char[30];

        public char[] Surname = new char[30];

        public char[] Patronymic = new char[30];

        public char[] Email = new char[30];

        public char[] Company = new char[30];

        public DateOnly ApplyDate { get; set; }

        public char[] Appointment = new char[30];

        public char[] City = new char[30];

        

    }

}