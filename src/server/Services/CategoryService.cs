using TaskSystem.Api.Dtos;
using TaskSystem.Api.Repositories;

namespace TaskSystem.Api.Services;

public class CategoryService(ICategoryRepository categoryRepository) : ICategoryService
{
    public async Task<IReadOnlyList<CategoryResponse>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetAllAsync(cancellationToken);
        return categories.Select(c => new CategoryResponse(c.Id, c.Name)).ToList();
    }
}
