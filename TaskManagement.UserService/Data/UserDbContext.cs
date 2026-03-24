using Microsoft.EntityFrameworkCore;
using TaskManagement.UserService.Models;

namespace TaskManagement.UserService.Data
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Seed default users (using BCrypt hashes for 'admin', 'manager', 'engineer')
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin"), Role = Roles.Admin },
                new User { Id = 2, Username = "manager", PasswordHash = BCrypt.Net.BCrypt.HashPassword("manager"), Role = Roles.Manager },
                new User { Id = 3, Username = "engineer", PasswordHash = BCrypt.Net.BCrypt.HashPassword("engineer"), Role = Roles.Engineer }
            );
        }
    }
}
