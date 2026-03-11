using System.ComponentModel.DataAnnotations;

namespace TaskManagement.TaskService.Models
{
    public class TaskItem
    {
        [Key]
        public int Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string Priority { get; set; } // Low, Medium, High
        public required string Status { get; set; } // Open, In Progress, Blocked, Completed
        public int? AssigneeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }

        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }

    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        public int TaskId { get; set; }
        public string? StatusChangedTo { get; set; }
        public int ChangedByUserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Navigation property
        public TaskItem? TaskItem { get; set; }
    }

    public static class TaskPriorities
    {
        public const string Low = "Low";
        public const string Medium = "Medium";
        public const string High = "High";
    }

    public static class TaskStatuses
    {
        public const string Open = "Open";
        public const string InProgress = "In Progress";
        public const string Blocked = "Blocked";
        public const string Completed = "Completed";
    }
}
