using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using TaskManagement.TaskService.Dtos;

namespace TaskManagement.Tests;

public class TaskServiceTests : IClassFixture<WebApplicationFactory<TaskManagement.TaskService.Program>>
{
    private readonly WebApplicationFactory<TaskManagement.TaskService.Program> _factory;

    public TaskServiceTests(WebApplicationFactory<TaskManagement.TaskService.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetTasks_ShouldReturnUnauthorized_WhenNoToken()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/api/tasks");
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
