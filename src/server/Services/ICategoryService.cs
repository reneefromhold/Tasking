using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Services;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
}
