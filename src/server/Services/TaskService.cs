using TaskSystem.Api.Auth;
using TaskSystem.Api.Dtos;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Extensions;
using TaskSystem.Api.Repositories;
using DomainTask = TaskSystem.Api.Entities.Task;

namespace TaskSystem.Api.Services;

public class TaskService(
    ITaskRepository taskRepository,
    IUserRepository userRepository,
    ICategoryRepository categoryRepository,
    ICurrentUserAccessor currentUserAccessor) : ITaskService
{
    public async Task<IReadOnlyList<TaskResponse>> GetTasksForCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var userId = await GetValidatedUserIdAsync(cancellationToken);
        var tasks = await taskRepository.GetByCreatorAsync(userId, cancellationToken);
        var responses = new List<TaskResponse>();
        foreach (var task in tasks)
        {
            responses.Add(await MapToResponseAsync(task, cancellationToken));
        }

        return responses;
    }

    public async Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = await GetValidatedUserIdAsync(cancellationToken);

        var categoryId = StringNormalization.NormalizeOptional(request.CategoryId);
        var assignee = StringNormalization.NormalizeOptional(request.Assignee);
        var description = StringNormalization.NormalizeOptional(request.Description);
        var dueDate = StringNormalization.NormalizeOptional(request.DueDate);

        await ValidateAssigneeAsync(assignee, cancellationToken);
        await ValidateCategoryAsync(categoryId, cancellationToken);

        var task = new DomainTask
        {
            Id = Guid.NewGuid().ToString(),
            CategoryId = categoryId,
            Title = request.Title.Trim(),
            Description = description,
            Creator = userId,
            Assignee = assignee,
            CreateDate = DateFormats.ToStorageString(DateTime.UtcNow),
            DueDate = dueDate
        };

        var created = await taskRepository.CreateAsync(task, cancellationToken);
        return await MapToResponseAsync(created, cancellationToken);
    }

    public async Task<TaskResponse> UpdateTaskAsync(string taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = await GetValidatedUserIdAsync(cancellationToken);
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);

        var categoryId = StringNormalization.NormalizeOptional(request.CategoryId);
        var assignee = StringNormalization.NormalizeOptional(request.Assignee);
        var description = StringNormalization.NormalizeOptional(request.Description);
        var dueDate = StringNormalization.NormalizeOptional(request.DueDate);

        await ValidateAssigneeAsync(assignee, cancellationToken);
        await ValidateCategoryAsync(categoryId, cancellationToken);

        task.CategoryId = categoryId;
        task.Title = request.Title.Trim();
        task.Description = description;
        task.Assignee = assignee;
        task.DueDate = dueDate;

        var updated = await taskRepository.UpdateAsync(task, cancellationToken);
        return await MapToResponseAsync(updated, cancellationToken);
    }

    public async Task DeleteTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        var userId = await GetValidatedUserIdAsync(cancellationToken);
        var task = await GetOwnedTaskAsync(taskId, userId, cancellationToken);
        await taskRepository.DeleteAsync(task, cancellationToken);
    }

    private async Task<string> GetValidatedUserIdAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();

        if (await userRepository.GetByIdAsync(userId, cancellationToken) is null)
        {
            throw new UnauthorizedAppException("User not found.");
        }

        return userId;
    }

    private async Task<DomainTask> GetOwnedTaskAsync(string taskId, string userId, CancellationToken cancellationToken)
    {
        var task = await taskRepository.GetByIdAsync(taskId, cancellationToken);

        if (task is null || task.Creator != userId)
        {
            throw new NotFoundException($"Task with id '{taskId}' was not found.");
        }

        return task;
    }

    private async Task ValidateAssigneeAsync(string? assigneeId, CancellationToken cancellationToken)
    {
        if (assigneeId is null)
        {
            return;
        }

        if (await userRepository.GetByIdAsync(assigneeId, cancellationToken) is null)
        {
            throw new NotFoundException($"User with id '{assigneeId}' was not found.");
        }
    }

    private async Task ValidateCategoryAsync(string? categoryId, CancellationToken cancellationToken)
    {
        if (categoryId is null)
        {
            return;
        }

        if (await categoryRepository.GetByIdAsync(categoryId, cancellationToken) is null)
        {
            throw new NotFoundException($"Category with id '{categoryId}' was not found.");
        }
    }

    private async Task<TaskResponse> MapToResponseAsync(DomainTask task, CancellationToken cancellationToken)
    {
        string? categoryName = null;
        if (task.CategoryId is not null)
        {
            var category = await categoryRepository.GetByIdAsync(task.CategoryId, cancellationToken);
            categoryName = category?.Name;
        }

        return new TaskResponse(
            task.Id,
            task.CategoryId,
            categoryName,
            task.Title,
            task.Description,
            task.Creator,
            task.Assignee,
            task.CreateDate,
            task.DueDate);
    }
}
