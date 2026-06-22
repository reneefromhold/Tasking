using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Services;

public interface IUserService
{
    Task<UserStatusResponse> GetStatusByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserSummaryResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
