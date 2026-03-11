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
            
            // Seed default users
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", PasswordHash = "admin", Role = Roles.Admin },
                new User { Id = 2, Username = "manager", PasswordHash = "manager", Role = Roles.Manager },
                new User { Id = 3, Username = "engineer", PasswordHash = "engineer", Role = Roles.Engineer }
            );
        }
    }
}
