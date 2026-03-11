namespace TaskManagement.ReportingService.Dtos
{
    // Fetched from other services
    public record UserDto(int Id, string Username, string Role);
    public record TaskDto(int Id, string Title, string? Description, string Priority, string Status, int? AssigneeId, DateTime CreatedAt, DateTime UpdatedAt, DateTime? DueDate);

    // Reporting outputs
    public record TasksByUserReportItem(int UserId, string Username, int TaskCount);
    public record TasksByStatusReportItem(string Status, int TaskCount);
    public record SLABreachReportItem(int TaskId, string TaskTitle, string AssigneeUsername, int DaysOverdue);
}
