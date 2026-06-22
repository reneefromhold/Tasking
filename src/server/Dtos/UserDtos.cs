using System.ComponentModel.DataAnnotations;

namespace TaskSystem.Api.Dtos;

public record UserStatusResponse(bool Exists, string Status, string? Id = null);

public record UserSummaryResponse(string Id, string Email, string FirstName, string LastName);

public record CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FirstName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; init; } = string.Empty;
}

public record UserResponse(string Id, string Email, string FirstName, string LastName);
