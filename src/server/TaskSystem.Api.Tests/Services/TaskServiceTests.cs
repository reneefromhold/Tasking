using Moq;
using TaskSystem.Api.Auth;
using TaskSystem.Api.Data;
using TaskSystem.Api.Dtos;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Repositories;
using TaskSystem.Api.Services;
using CategoryEntity = TaskSystem.Api.Entities.Category;
using DomainTask = TaskSystem.Api.Entities.Task;
using UserEntity = TaskSystem.Api.Entities.User;

namespace TaskSystem.Api.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor = new();
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _sut = new TaskService(
            _taskRepository.Object,
            _userRepository.Object,
            _categoryRepository.Object,
            _currentUserAccessor.Object);
    }

    [Fact]
    public async Task GetTasksForCurrentUserAsync_ReturnsTasksForCurrentUser()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var task = CreateTask(creatorId: SeedIds.DemoUserId, categoryId: SeedIds.Epic1CategoryId);
        _taskRepository
            .Setup(r => r.GetByCreatorAsync(SeedIds.DemoUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DomainTask> { task });
        SetupCategory(SeedIds.Epic1CategoryId, "Epic 1");

        var results = await _sut.GetTasksForCurrentUserAsync();

        Assert.Single(results);
        Assert.Equal(task.Id, results[0].Id);
        Assert.Equal(task.Title, results[0].Title);
        Assert.Equal("Epic 1", results[0].CategoryName);
    }

    [Fact]
    public async Task GetTasksForCurrentUserAsync_ReturnsEmptyListWhenUserHasNoTasks()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        _taskRepository
            .Setup(r => r.GetByCreatorAsync(SeedIds.DemoUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<DomainTask>());

        var results = await _sut.GetTasksForCurrentUserAsync();

        Assert.Empty(results);
    }

    [Fact]
    public async Task GetTasksForCurrentUserAsync_ThrowsWhenUserIdMissing()
    {
        _currentUserAccessor
            .Setup(a => a.GetRequiredUserId())
            .Throws(new UnauthorizedAppException("Missing header."));

        await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _sut.GetTasksForCurrentUserAsync());
    }

    [Fact]
    public async Task GetTasksForCurrentUserAsync_ThrowsWhenUserNotFound()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        _userRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        await Assert.ThrowsAsync<UnauthorizedAppException>(
            () => _sut.GetTasksForCurrentUserAsync());
    }

    [Fact]
    public async Task CreateTaskAsync_CreatesTaskWithCurrentUserAsCreator()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        SetupCategory(SeedIds.Epic1CategoryId, "Epic 1");

        DomainTask? captured = null;
        _taskRepository
            .Setup(r => r.CreateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .Callback<DomainTask, CancellationToken>((task, _) => captured = task)
            .ReturnsAsync((DomainTask task, CancellationToken _) => task);

        var request = new CreateTaskRequest
        {
            CategoryId = SeedIds.Epic1CategoryId,
            Title = "  New task  ",
            Description = "  Details  ",
            DueDate = "2026-12-31T23:59:59Z"
        };

        var result = await _sut.CreateTaskAsync(request);

        Assert.NotNull(captured);
        Assert.Equal(SeedIds.DemoUserId, captured!.Creator);
        Assert.Equal(SeedIds.Epic1CategoryId, captured.CategoryId);
        Assert.Equal("New task", captured.Title);
        Assert.Equal("Details", captured.Description);
        Assert.Equal("2026-12-31T23:59:59Z", captured.DueDate);
        Assert.False(string.IsNullOrWhiteSpace(captured.CreateDate));
        Assert.Equal("New task", result.Title);
        Assert.Equal("Epic 1", result.CategoryName);
    }

    [Fact]
    public async Task CreateTaskAsync_AllowsNullOptionalFields()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        DomainTask? captured = null;
        _taskRepository
            .Setup(r => r.CreateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .Callback<DomainTask, CancellationToken>((task, _) => captured = task)
            .ReturnsAsync((DomainTask task, CancellationToken _) => task);

        var request = new CreateTaskRequest { Title = "Minimal task" };

        var result = await _sut.CreateTaskAsync(request);

        Assert.NotNull(captured);
        Assert.Null(captured!.CategoryId);
        Assert.Null(captured.Assignee);
        Assert.Null(captured.Description);
        Assert.Null(result.CategoryName);
    }

    [Fact]
    public async Task CreateTaskAsync_StoresWhitespaceOptionalFieldsAsNull()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        DomainTask? captured = null;
        _taskRepository
            .Setup(r => r.CreateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .Callback<DomainTask, CancellationToken>((task, _) => captured = task)
            .ReturnsAsync((DomainTask task, CancellationToken _) => task);

        var request = new CreateTaskRequest
        {
            Title = "Task",
            CategoryId = "   ",
            Assignee = "   ",
            Description = "   ",
            DueDate = "   "
        };

        await _sut.CreateTaskAsync(request);

        Assert.NotNull(captured);
        Assert.Null(captured!.CategoryId);
        Assert.Null(captured.Assignee);
        Assert.Null(captured.Description);
        Assert.Null(captured.DueDate);
    }

    [Fact]
    public async Task UpdateTaskAsync_StoresWhitespaceOptionalFieldsAsNull()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var existing = CreateTask(
            id: SeedIds.DemoTask1Id,
            creatorId: SeedIds.DemoUserId,
            categoryId: SeedIds.Epic1CategoryId,
            title: "Old title");

        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _taskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTask task, CancellationToken _) => task);

        var request = new UpdateTaskRequest
        {
            Title = "Updated",
            CategoryId = "   ",
            Assignee = "   "
        };

        await _sut.UpdateTaskAsync(SeedIds.DemoTask1Id, request);

        Assert.Null(existing.CategoryId);
        Assert.Null(existing.Assignee);
    }

    [Fact]
    public async Task CreateTaskAsync_ThrowsWhenAssigneeNotFound()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        _userRepository
            .Setup(r => r.GetByIdAsync("missing-assignee", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var request = new CreateTaskRequest
        {
            Title = "Task",
            Assignee = "missing-assignee"
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateTaskAsync(request));
    }

    [Fact]
    public async Task CreateTaskAsync_ThrowsWhenCategoryNotFound()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        _categoryRepository
            .Setup(r => r.GetByIdAsync("missing-category", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryEntity?)null);

        var request = new CreateTaskRequest
        {
            Title = "Task",
            CategoryId = "missing-category"
        };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.CreateTaskAsync(request));
    }

    [Fact]
    public async Task UpdateTaskAsync_UpdatesOwnedTask()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        SetupCategory(SeedIds.Epic2CategoryId, "Epic 2");

        var existing = CreateTask(
            id: SeedIds.DemoTask1Id,
            creatorId: SeedIds.DemoUserId,
            categoryId: SeedIds.Epic1CategoryId,
            title: "Old title");

        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _taskRepository
            .Setup(r => r.UpdateAsync(It.IsAny<DomainTask>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTask task, CancellationToken _) => task);

        var request = new UpdateTaskRequest
        {
            CategoryId = SeedIds.Epic2CategoryId,
            Title = "  Updated title  ",
            Description = "Updated details",
            DueDate = "2027-01-01T00:00:00Z"
        };

        var result = await _sut.UpdateTaskAsync(SeedIds.DemoTask1Id, request);

        Assert.Equal("Updated title", existing.Title);
        Assert.Equal("Updated details", existing.Description);
        Assert.Equal(SeedIds.Epic2CategoryId, existing.CategoryId);
        Assert.Equal("2027-01-01T00:00:00Z", existing.DueDate);
        Assert.Equal("Updated title", result.Title);
        Assert.Equal("Epic 2", result.CategoryName);
        _taskRepository.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowsNotFoundWhenTaskDoesNotExist()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        _taskRepository
            .Setup(r => r.GetByIdAsync("missing-task", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTask?)null);

        var request = new UpdateTaskRequest { Title = "Updated" };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateTaskAsync("missing-task", request));
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowsNotFoundWhenTaskBelongsToAnotherUser()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var otherUsersTask = CreateTask(
            id: SeedIds.DemoTask1Id,
            creatorId: "other-user-id",
            title: "Not yours");

        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUsersTask);

        var request = new UpdateTaskRequest { Title = "Hijacked" };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateTaskAsync(SeedIds.DemoTask1Id, request));
    }

    [Fact]
    public async Task UpdateTaskAsync_ThrowsWhenAssigneeNotFound()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var existing = CreateTask(id: SeedIds.DemoTask1Id, creatorId: SeedIds.DemoUserId);
        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _userRepository
            .Setup(r => r.GetByIdAsync("missing-assignee", It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserEntity?)null);

        var request = new UpdateTaskRequest
        {
            Title = "Updated",
            Assignee = "missing-assignee"
        };

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.UpdateTaskAsync(SeedIds.DemoTask1Id, request));
    }

    [Fact]
    public async Task DeleteTaskAsync_DeletesOwnedTask()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var existing = CreateTask(id: SeedIds.DemoTask1Id, creatorId: SeedIds.DemoUserId);
        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.DeleteTaskAsync(SeedIds.DemoTask1Id);

        _taskRepository.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteTaskAsync_ThrowsNotFoundWhenTaskDoesNotExist()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);
        _taskRepository
            .Setup(r => r.GetByIdAsync("missing-task", It.IsAny<CancellationToken>()))
            .ReturnsAsync((DomainTask?)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteTaskAsync("missing-task"));
    }

    [Fact]
    public async Task DeleteTaskAsync_ThrowsNotFoundWhenTaskBelongsToAnotherUser()
    {
        SetupCurrentUser(SeedIds.DemoUserId);
        SetupUserExists(SeedIds.DemoUserId);

        var otherUsersTask = CreateTask(id: SeedIds.DemoTask1Id, creatorId: "other-user-id");
        _taskRepository
            .Setup(r => r.GetByIdAsync(SeedIds.DemoTask1Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherUsersTask);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.DeleteTaskAsync(SeedIds.DemoTask1Id));
    }

    private void SetupCurrentUser(string userId)
    {
        _currentUserAccessor.Setup(a => a.GetRequiredUserId()).Returns(userId);
    }

    private void SetupUserExists(string userId)
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserEntity { Id = userId, Email = "user@example.com" });
    }

    private void SetupCategory(string categoryId, string name)
    {
        _categoryRepository
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CategoryEntity { Id = categoryId, Name = name });
    }

    private static DomainTask CreateTask(
        string? id = null,
        string? creatorId = null,
        string? categoryId = null,
        string title = "Sample task")
    {
        return new DomainTask
        {
            Id = id ?? Guid.NewGuid().ToString(),
            Creator = creatorId,
            CategoryId = categoryId,
            Title = title,
            CreateDate = "2026-01-01T00:00:00Z"
        };
    }
}
