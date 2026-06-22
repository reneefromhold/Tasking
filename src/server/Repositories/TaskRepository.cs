using TaskSystem.Api.Data;
using Microsoft.EntityFrameworkCore;
using DomainTask = TaskSystem.Api.Entities.Task;

namespace TaskSystem.Api.Repositories;

public class TaskRepository(AppDbContext context) : ITaskRepository
{
    public async Task<IReadOnlyList<DomainTask>> GetByCreatorAsync(string creatorId, CancellationToken cancellationToken = default) =>
        await context.Tasks
            .Where(t => t.Creator == creatorId)
            .OrderByDescending(t => t.CreateDate)
            .ToListAsync(cancellationToken);

    public Task<DomainTask?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<DomainTask> CreateAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Add(task);
        await context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<DomainTask> UpdateAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Update(task);
        await context.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task DeleteAsync(DomainTask task, CancellationToken cancellationToken = default)
    {
        context.Tasks.Remove(task);
        await context.SaveChangesAsync(cancellationToken);
    }
}
