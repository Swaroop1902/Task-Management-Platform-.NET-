using System.ComponentModel.DataAnnotations;

namespace TaskManagement.TaskService.Dtos
{
    public record CreateTaskRequest(
        [Required] string Title, 
        string? Description, 
        [Required] string Priority, 
        string Status, 
        int? AssigneeId, 
        DateTime? DueDate);
    public record UpdateTaskRequest(
        string? Title, 
        string? Description, 
        string? Priority, 
        string? Status, 
        int? AssigneeId, 
        DateTime? DueDate);
    
    public record ActivityLogDto(int Id, int TaskId, string? StatusChangedTo, int ChangedByUserId, DateTime Timestamp);
    public record TaskDto(int Id, string Title, string? Description, string Priority, string Status, int? AssigneeId, DateTime CreatedAt, DateTime UpdatedAt, DateTime? DueDate, IEnumerable<ActivityLogDto> ActivityLogs);
}
