namespace TaskSystem.Api.Repositories;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IReadOnlyList<TaskSystem.Api.Entities.Task>> GetByCreatorAsync(string creatorId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<TaskSystem.Api.Entities.Task?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<TaskSystem.Api.Entities.Task> CreateAsync(TaskSystem.Api.Entities.Task task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<TaskSystem.Api.Entities.Task> UpdateAsync(TaskSystem.Api.Entities.Task task, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task DeleteAsync(TaskSystem.Api.Entities.Task task, CancellationToken cancellationToken = default);
}
