using TaskSystem.Api.Data;
using TaskSystem.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace TaskSystem.Api.Repositories;

public class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

    public Task<Category?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        context.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
}
