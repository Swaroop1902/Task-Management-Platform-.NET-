using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TaskManagement.UserService.Dtos;

namespace TaskManagement.Tests;

public class UserServiceTests : IClassFixture<WebApplicationFactory<TaskManagement.UserService.Program>>
{
    private readonly WebApplicationFactory<TaskManagement.UserService.Program> _factory;

    public UserServiceTests(WebApplicationFactory<TaskManagement.UserService.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_WithWrongCredentials_ShouldReturnUnauthorized()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/auth/login", new LoginRequest("admin", "wrongpassword"));
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
