using TaskSystem.Api.Exceptions;

namespace TaskSystem.Api.Auth;

public class HeaderCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public const string UserIdHeaderName = "X-User-Id";

    public string? UserId
    {
        get
        {
            var header = httpContextAccessor.HttpContext?.Request.Headers[UserIdHeaderName].FirstOrDefault();
            return string.IsNullOrWhiteSpace(header) ? null : header.Trim();
        }
    }

    public string GetRequiredUserId() =>
        UserId ?? throw new UnauthorizedAppException($"Missing or invalid {UserIdHeaderName} header.");
}
