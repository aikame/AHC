using Frontend.Models;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Context
{
    public class AHCContext : DbContext
    {
        public DbSet<AppointmentModel> Appointments => Set<AppointmentModel>();
        public DbSet<CitiesModel> Cities => Set<CitiesModel>();
        public DbSet<CompaniesModel> Companies => Set<CompaniesModel>();
        public DbSet <DepartmentModel> Departments => Set<DepartmentModel>();

        public AHCContext(DbContextOptions<AHCContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
