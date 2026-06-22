using TaskSystem.Api.Dtos;
using TaskSystem.Api.Entities;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Repositories;

namespace TaskSystem.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<UserStatusResponse> GetStatusByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return new UserStatusResponse(Exists: false, Status: "not_found");
        }

        return new UserStatusResponse(
            Exists: true,
            Status: "active",
            Id: user.Id);
    }

    public async Task<IReadOnlyList<UserSummaryResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Select(u => new UserSummaryResponse(u.Id, u.Email, u.FirstName, u.LastName)).ToList();
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (await userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            throw new ConflictException($"A user with email '{normalizedEmail}' already exists.");
        }

        var user = new User
        {
            Id = Guid.NewGuid().ToString(),
            Email = normalizedEmail,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim()
        };

        var created = await userRepository.CreateAsync(user, cancellationToken);
        return MapToResponse(created);
    }

    private static string NormalizeEmail(string email) =>
        email.Trim().ToLowerInvariant();

    private static UserResponse MapToResponse(User user) =>
        new(user.Id, user.Email, user.FirstName, user.LastName);
}
