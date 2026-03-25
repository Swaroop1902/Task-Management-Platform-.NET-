using Microsoft.EntityFrameworkCore;
using TaskManagement.TaskService.Models;

namespace TaskManagement.TaskService.Data
{
    public class TaskDbContext : DbContext
    {
        public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<ActivityLog>()
                .HasOne(a => a.TaskItem)
                .WithMany(t => t.ActivityLogs)
                .HasForeignKey(a => a.TaskId);
            
            // Seed a sample task
            modelBuilder.Entity<TaskItem>().HasData(
                new TaskItem 
                { 
                    Id = 1, 
                    Title = "Initial Task", 
                    Description = "Setup microservices", 
                    Priority = TaskPriorities.High, 
                    Status = TaskStatuses.InProgress,
                    AssigneeId = 3, // Engineer
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(2)
                },
                new TaskItem 
                { 
                    Id = 2, 
                    Title = "Overdue Task", 
                    Description = "This task should trigger an SLA breach", 
                    Priority = TaskPriorities.Medium, 
                    Status = TaskStatuses.Open,
                    AssigneeId = 1, // Admin
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    UpdatedAt = DateTime.UtcNow.AddDays(-5),
                    DueDate = DateTime.UtcNow.AddDays(-1)
                },
                new TaskItem
                {
                    Id = 3,
                    Title = "UI Implementation",
                    Description = "Build dashboard using Angular",
                    Priority = TaskPriorities.High,
                    Status = TaskStatuses.Blocked,
                    AssigneeId = 2, // Manager
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(5)
                },
                new TaskItem
                {
                    Id = 4,
                    Title = "Documentation",
                    Description = "Write internal API documentation",
                    Priority = TaskPriorities.Low,
                    Status = TaskStatuses.Completed,
                    AssigneeId = 3, // Engineer
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2),
                    DueDate = DateTime.UtcNow.AddDays(-5)
                }
            );

            modelBuilder.Entity<ActivityLog>().HasData(
                new ActivityLog
                {
                    Id = 1,
                    TaskId = 1,
                    StatusChangedTo = TaskStatuses.InProgress,
                    ChangedByUserId = 3,
                    Timestamp = DateTime.UtcNow.AddHours(-5)
                },
                new ActivityLog
                {
                    Id = 2,
                    TaskId = 3,
                    StatusChangedTo = TaskStatuses.Open,
                    ChangedByUserId = 2,
                    Timestamp = DateTime.UtcNow.AddDays(-2)
                },
                new ActivityLog
                {
                    Id = 3,
                    TaskId = 3,
                    StatusChangedTo = TaskStatuses.Blocked,
                    ChangedByUserId = 1,
                    Timestamp = DateTime.UtcNow.AddHours(-1)
                }
            );
        }
    }
}
