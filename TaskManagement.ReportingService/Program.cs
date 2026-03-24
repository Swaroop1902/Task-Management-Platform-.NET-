using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagement.ReportingService.Dtos;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var jwtSecret = builder.Configuration["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured.");
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

var reportGroup = app.MapGroup("/api/reports").RequireAuthorization();

// For simplicity, we assume the services match the docker layout, or local ports. 
// We will grab the services URLs from config.
// In docker, they will be "http://userservice:80" and "http://taskservice:80".
// In local, they might be "http://localhost:5001" etc.
// We will inject the token through the headers.

async Task<List<TaskDto>> FetchTasksFromTaskService(IHttpClientFactory httpClientFactory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
{
    var client = httpClientFactory.CreateClient();
    var taskServiceUrl = config["ServiceUrls:TaskService"] ?? "http://localhost:5045";
    
    // Pass the auth token
    var token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Add("Authorization", token);
    }

    var response = await client.GetAsync($"{taskServiceUrl}/api/tasks");
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<List<TaskDto>>() ?? new List<TaskDto>();
}

async Task<List<UserDto>> FetchUsersFromUserService(IHttpClientFactory httpClientFactory, IConfiguration config, IHttpContextAccessor httpContextAccessor)
{
    var client = httpClientFactory.CreateClient();
    var userServiceUrl = config["ServiceUrls:UserService"] ?? "http://localhost:5147";
    
    // Pass the auth token
    var token = httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();
    if (!string.IsNullOrEmpty(token))
    {
        client.DefaultRequestHeaders.Add("Authorization", token);
    }

    var response = await client.GetAsync($"{userServiceUrl}/api/users");
    response.EnsureSuccessStatusCode();
    
    return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
}



reportGroup.MapGet("/tasks-by-user", async (IHttpClientFactory clientFactory, IConfiguration config, IHttpContextAccessor httpContextAccessor, IMemoryCache cache) =>
{
    return await cache.GetOrCreateAsync("TasksByUser", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
        
        var tasks = await FetchTasksFromTaskService(clientFactory, config, httpContextAccessor);
        var users = await FetchUsersFromUserService(clientFactory, config, httpContextAccessor);

        var report = users.Select(u => new TasksByUserReportItem(
            u.Id,
            u.Username,
            tasks.Count(t => t.AssigneeId == u.Id)
        )).ToList();

        return Results.Ok(report);
    });
});

reportGroup.MapGet("/tasks-by-status", async (IHttpClientFactory clientFactory, IConfiguration config, IHttpContextAccessor httpContextAccessor, IMemoryCache cache) =>
{
    return await cache.GetOrCreateAsync("TasksByStatus", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
        
        var tasks = await FetchTasksFromTaskService(clientFactory, config, httpContextAccessor);

        var report = tasks.GroupBy(t => t.Status)
            .Select(g => new TasksByStatusReportItem(g.Key, g.Count()))
            .ToList();

        return Results.Ok(report);
    });
});

reportGroup.MapGet("/sla-breaches", async (IHttpClientFactory clientFactory, IConfiguration config, IHttpContextAccessor httpContextAccessor, IMemoryCache cache) =>
{
    return await cache.GetOrCreateAsync("SLABreaches", async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
        
        var tasks = await FetchTasksFromTaskService(clientFactory, config, httpContextAccessor);
        var users = await FetchUsersFromUserService(clientFactory, config, httpContextAccessor);

        var now = DateTime.UtcNow;
        var overdueTasks = tasks.Where(t => t.DueDate < now && t.Status != "Completed").ToList();

        var report = overdueTasks.Select(t => {
            var assignee = users.FirstOrDefault(u => u.Id == t.AssigneeId);
            var daysOverdue = (now - t.DueDate!.Value).Days;
            return new SLABreachReportItem(t.Id, t.Title, assignee?.Username ?? "Unassigned", daysOverdue);
        }).ToList();

        return Results.Ok(report);
    });
});

app.Run();

namespace TaskManagement.ReportingService { public partial class Program { } }
