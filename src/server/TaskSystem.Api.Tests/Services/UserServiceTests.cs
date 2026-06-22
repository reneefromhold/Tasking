using Moq;
using TaskSystem.Api.Dtos;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Repositories;
using TaskSystem.Api.Services;
using UserEntity = TaskSystem.Api.Entities.User;

namespace TaskSystem.Api.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository.Object);
    }

    [Fact]
    public async Task GetStatusByEmailAsync_ReturnsActiveWhenUserExists()
    {
        var user = new UserEntity
        {
            Id = "user-1",
            Email = "demo@tasksystem.com",
            FirstName = "Demo",
            LastName = "User"
        };

        _userRepository
            .Setup(r => r.GetByEmailAsync("demo@tasksystem.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetStatusByEmailAsync("demo@tasksystem.com");

        Assert.True(result.Exists);
        Assert.Equal("active", result.Status);
        Assert.Equal("user-1", result.Id);
    }

    [Fact]
    public async Task GetStatusByEmailAsync_ReturnsNotFoundWhenUserMissing()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("missing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var result = await _sut.GetStatusByEmailAsync("missing@example.com");

        Assert.False(result.Exists);
        Assert.Equal("not_found", result.Status);
        Assert.Null(result.Id);
    }

    [Fact]
    public async Task GetStatusByEmailAsync_NormalizesEmail()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("demo@tasksystem.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        await _sut.GetStatusByEmailAsync("  Demo@TaskSystem.com  ");

        _userRepository.Verify(
            r => r.GetByEmailAsync("demo@tasksystem.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_CreatesUser()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync("new@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        UserEntity? captured = null;
        _userRepository
            .Setup(r => r.CreateAsync(It.IsAny<UserEntity>(), It.IsAny<CancellationToken>()))
            .Callback<UserEntity, CancellationToken>((user, _) => captured = user)
            .ReturnsAsync((UserEntity user, CancellationToken _) => user);

        var request = new CreateUserRequest
        {
            Email = "  New@Example.com  ",
            FirstName = "  Jane  ",
            LastName = "  Doe  "
        };

        var result = await _sut.CreateUserAsync(request);

        Assert.NotNull(captured);
        Assert.Equal("new@example.com", captured!.Email);
        Assert.Equal("Jane", captured.FirstName);
        Assert.Equal("Doe", captured.LastName);
        Assert.False(string.IsNullOrWhiteSpace(captured.Id));
        Assert.Equal(captured.Id, result.Id);
        Assert.Equal("new@example.com", result.Email);
    }

    [Fact]
    public async Task CreateUserAsync_ThrowsConflictWhenEmailExists()
    {
        _userRepository
            .Setup(r => r.EmailExistsAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateUserRequest
        {
            Email = "existing@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        await Assert.ThrowsAsync<ConflictException>(() => _sut.CreateUserAsync(request));
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsMappedUsers()
    {
        var users = new List<UserEntity>
        {
            new() { Id = "1", Email = "a@example.com", FirstName = "A", LastName = "One" },
            new() { Id = "2", Email = "b@example.com", FirstName = "B", LastName = "Two" }
        };

        _userRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var results = await _sut.GetAllUsersAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("a@example.com", results[0].Email);
        Assert.Equal("B", results[1].FirstName);
    }
}
