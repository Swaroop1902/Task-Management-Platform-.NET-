namespace TaskManagement.UserService.Dtos
{
    public record UserDto(int Id, string Username, string Role);
    public record CreateUserRequest(string Username, string Password, string Role);
    public record UpdateUserRequest(string Username, string Role);
}
