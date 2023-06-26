using Frontend.Context;
using Frontend.Interfaces;
using Frontend.Models;

namespace Frontend.Classes
{
    public class AHCDatabase : IAHCDatabase
    {
        readonly AHCContext _context;

        public AHCDatabase ( AHCContext context )
        {
            _context = context;
        }

        public List<AppointmentModel> GetAllAppointment()
        {
            return _context.Appointments.AsEnumerable().ToList();
        }
        public List<CitiesModel> GetAllCities()
        {
            return _context.Cities.AsEnumerable().ToList();
        }
        public List<CompaniesModel> GetAllCompanies()
        {
            return _context.Companies.AsEnumerable().ToList();
        }
        public List<DepartmentModel> GetAllDepartments()
        {
            return _context.Departments.AsEnumerable().ToList();
        }
    }
}
