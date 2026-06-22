using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Services;

public interface ITaskService
{
    Task<IReadOnlyList<TaskResponse>> GetTasksForCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<TaskResponse> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);
    Task<TaskResponse> UpdateTaskAsync(string taskId, UpdateTaskRequest request, CancellationToken cancellationToken = default);
    Task DeleteTaskAsync(string taskId, CancellationToken cancellationToken = default);
}
