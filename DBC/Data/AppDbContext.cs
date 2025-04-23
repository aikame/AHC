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
        public DbSet<GroupModel> Groups { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ADAccountModel>()
                .HasOne(a => a.Profile)
                .WithMany(p => p.ADAccounts)
                .HasForeignKey(a => a.ProfileModelId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComputerModel>()
                .Property(e => e.DiskSpace)
                .HasColumnType("jsonb[]");

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
