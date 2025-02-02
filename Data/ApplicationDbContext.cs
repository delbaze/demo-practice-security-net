using Microsoft.EntityFrameworkCore;
using DemoPracticeSecurityNet.Models;

namespace DemoPracticeSecurityNet.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    UserId = 1,
                    Username = "admin",
                    Password = "admin123",
                    Email = "admin@company.com",
                    CreditCard = "4532-7165-9087-2342",
                    IsAdmin = true,
                    PrivateNotes = "Mes notes secrètes d'admin"
                },
                new User
                {
                    Id = 2,
                    UserId = 2,
                    Username = "john.doe",
                    Password = "password123",
                    Email = "john@example.com",
                    CreditCard = "4539-5789-3456-2190",
                    IsAdmin = false,
                    PrivateNotes = "Mes notes personnelles"
                }
            );
        }
    }
}