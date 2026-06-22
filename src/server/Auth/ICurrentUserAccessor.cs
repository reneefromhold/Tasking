namespace TaskSystem.Api.Auth;

/// <summary>
/// Abstraction for resolving the authenticated user.
/// Currently reads X-User-Id from the request header.
/// Replace the implementation with a real auth provider if added later.
/// </summary>
public interface ICurrentUserAccessor
{
    string? UserId { get; }
    string GetRequiredUserId();
}
