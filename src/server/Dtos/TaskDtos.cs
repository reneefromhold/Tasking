using System.ComponentModel.DataAnnotations;

namespace TaskSystem.Api.Dtos;

public record CreateTaskRequest
{
    public string? CategoryId { get; init; }

    [Required]
    [MaxLength(30)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    public string? Assignee { get; init; }

    [MaxLength(25)]
    public string? DueDate { get; init; }
}

public record UpdateTaskRequest
{
    public string? CategoryId { get; init; }

    [Required]
    [MaxLength(30)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; init; }

    public string? Assignee { get; init; }

    [MaxLength(25)]
    public string? DueDate { get; init; }
}

public record TaskResponse(
    string Id,
    string? CategoryId,
    string? CategoryName,
    string Title,
    string? Description,
    string? Creator,
    string? Assignee,
    string CreateDate,
    string? DueDate);
