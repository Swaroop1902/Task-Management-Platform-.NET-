using System.ComponentModel.DataAnnotations;

namespace TaskManagement.UserService.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string PasswordHash { get; set; }
        public required string Role { get; set; } // Admin, Manager, Engineer
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Engineer = "Engineer";
    }
}
