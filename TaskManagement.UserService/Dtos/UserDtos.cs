using System.ComponentModel.DataAnnotations;

namespace TaskManagement.UserService.Dtos
{
    public record UserDto(int Id, string Username, string Role);
    public record CreateUserRequest(
        [Required] string Username, 
        [Required, MinLength(4)] string Password, 
        [Required] string Role);
    public record UpdateUserRequest(
        [Required] string Username, 
        [Required] string Role);
}
