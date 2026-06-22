namespace TaskSystem.Api.Entities;

public class Task
{
    public string Id { get; set; } = string.Empty;
    public string? CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Creator { get; set; }
    public string? Assignee { get; set; }
    public string CreateDate { get; set; } = string.Empty;
    public string? DueDate { get; set; }
}
