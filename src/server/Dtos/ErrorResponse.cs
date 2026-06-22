namespace TaskSystem.Api.Dtos;

public record ErrorResponse(string Message, string? Detail = null);
