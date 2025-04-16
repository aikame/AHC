using DBC.Models.PostgreSQL;
using DBC.Models.Shared;
using Microsoft.EntityFrameworkCore;
namespace DBC.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<ProfileModel> Profiles { get; set; }
        public DbSet<ADAccountModel> ADAccounts { get; set; }
        public DbSet<ComputerModel> Computers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ADAccountModel>()
                .HasOne(a => a.Profile)
                .WithMany()
                .HasForeignKey(a => a.ProfileModelId)
                .OnDelete(DeleteBehavior.Cascade);

            // JsonElement сериализация (если надо)
            modelBuilder.Entity<ComputerModel>()
                .Property(e => e.DiskSpace)
                .HasColumnType("jsonb[]");

            // Массивы строк и чисел
            modelBuilder.Entity<ComputerModel>()
                .Property(e => e.CPUName)
                .HasColumnType("text[]");

            modelBuilder.Entity<ComputerModel>()
                .Property(e => e.CPUCores)
                .HasColumnType("integer[]");

            base.OnModelCreating(modelBuilder);
        }
    }
}
