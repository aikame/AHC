using Frontend.Models

namespace Frontend.Interfaces
{
    public interface IAHCDatabase
    {
        public CitiesModel GetCityById(int id);
        public DepartmentModel GetDepartmentByCity(CitiesModel City);
    }
}
