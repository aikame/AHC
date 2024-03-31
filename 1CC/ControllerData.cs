namespace _1CC
{

    public class ReceivedData
    {
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Patronymic { get; set; }

        public string? Email { get; set; }

        public string? Company { get; set; }

        public string? ApplyDate { get; set; }

        public string? Appointment { get; set; }

        public string? City { get; set; }

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