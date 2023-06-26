using Frontend.Models;

namespace Frontend.Interfaces
{
    public interface IAHCDatabase
    {
        public List<AppointmentModel> GetAllAppointment();
        public List<CitiesModel> GetAllCities();
        public List<CompaniesModel> GetAllCompanies();
        public List<DepartmentModel> GetAllDepartments();
    }
}
