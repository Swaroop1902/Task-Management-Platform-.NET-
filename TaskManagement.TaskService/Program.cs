using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using TaskManagement.TaskService.Data;
using TaskManagement.TaskService.Dtos;
using TaskManagement.TaskService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseInMemoryDatabase("TaskDb"));

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

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    dbContext.Database.EnsureCreated();
}

var taskGroup = app.MapGroup("/api/tasks").RequireAuthorization();

taskGroup.MapGet("/", async (string? status, int? assigneeId, DateTime? startDate, DateTime? endDate, TaskDbContext db) =>
{
    var query = db.Tasks.Include(t => t.ActivityLogs).AsQueryable();

    if (!string.IsNullOrEmpty(status)) query = query.Where(t => t.Status == status);
    if (assigneeId.HasValue) query = query.Where(t => t.AssigneeId == assigneeId.Value);
    if (startDate.HasValue) query = query.Where(t => t.CreatedAt >= startDate.Value);
    if (endDate.HasValue) query = query.Where(t => t.CreatedAt <= endDate.Value);

    var tasks = await query.ToListAsync();

    var result = tasks.Select(t => new TaskDto(
        t.Id, t.Title, t.Description, t.Priority, t.Status, t.AssigneeId, t.CreatedAt, t.UpdatedAt, t.DueDate,
        t.ActivityLogs.Select(a => new ActivityLogDto(a.Id, a.TaskId, a.StatusChangedTo, a.ChangedByUserId, a.Timestamp))
    ));

    return Results.Ok(result);
});

taskGroup.MapGet("/{id}", async (int id, TaskDbContext db) =>
{
    var task = await db.Tasks.Include(t => t.ActivityLogs).FirstOrDefaultAsync(t => t.Id == id);
    if (task == null) return Results.NotFound();

    var dto = new TaskDto(
        task.Id, task.Title, task.Description, task.Priority, task.Status, task.AssigneeId, task.CreatedAt, task.UpdatedAt, task.DueDate,
        task.ActivityLogs.Select(a => new ActivityLogDto(a.Id, a.TaskId, a.StatusChangedTo, a.ChangedByUserId, a.Timestamp))
    );
    return Results.Ok(dto);
});

taskGroup.MapPost("/", async (CreateTaskRequest req, ClaimsPrincipal user, TaskDbContext db) =>
{
    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    _ = int.TryParse(userIdString, out int userId);

    var task = new TaskItem
    {
        Title = req.Title,
        Description = req.Description,
        Priority = req.Priority,
        Status = req.Status ?? TaskStatuses.Open,
        AssigneeId = req.AssigneeId,
        DueDate = req.DueDate,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    db.Tasks.Add(task);
    
    // Initial activity log
    db.ActivityLogs.Add(new ActivityLog
    {
        TaskItem = task,
        StatusChangedTo = task.Status,
        ChangedByUserId = userId,
        Timestamp = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
    
    // Map to Dto not really necessary for post response unless needed, returning empty for simplicity
    return Results.Created($"/api/tasks/{task.Id}", task.Id);
});

taskGroup.MapPut("/{id}", async (int id, UpdateTaskRequest req, ClaimsPrincipal user, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    _ = int.TryParse(userIdString, out int userId);

    bool statusChanged = !string.IsNullOrEmpty(req.Status) && task.Status != req.Status;

    if (!string.IsNullOrEmpty(req.Title)) task.Title = req.Title;
    if (req.Description != null) task.Description = req.Description;
    if (!string.IsNullOrEmpty(req.Priority)) task.Priority = req.Priority;
    
    if (statusChanged)
    {
        task.Status = req.Status!;
        db.ActivityLogs.Add(new ActivityLog
        {
            TaskId = task.Id,
            StatusChangedTo = task.Status,
            ChangedByUserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    if (req.AssigneeId.HasValue) task.AssigneeId = req.AssigneeId;
    if (req.DueDate.HasValue) task.DueDate = req.DueDate.Value;

    task.UpdatedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();
    return Results.NoContent();
});

taskGroup.MapDelete("/{id}", async (int id, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization(p => p.RequireRole(Roles.Admin, Roles.Manager));

app.Run();

namespace TaskManagement.TaskService { public partial class Program { } }
