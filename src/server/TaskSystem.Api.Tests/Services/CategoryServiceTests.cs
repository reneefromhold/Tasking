using Moq;
using TaskSystem.Api.Repositories;
using TaskSystem.Api.Services;
using CategoryEntity = TaskSystem.Api.Entities.Category;

namespace TaskSystem.Api.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepository = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests()
    {
        _sut = new CategoryService(_categoryRepository.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsMappedCategories()
    {
        var categories = new List<CategoryEntity>
        {
            new() { Id = "cat-1", Name = "Epic 1" },
            new() { Id = "cat-2", Name = "Epic 2" }
        };

        _categoryRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var results = await _sut.GetAllCategoriesAsync();

        Assert.Equal(2, results.Count);
        Assert.Equal("cat-1", results[0].Id);
        Assert.Equal("Epic 1", results[0].Category);
        Assert.Equal("Epic 2", results[1].Category);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsEmptyListWhenNoCategories()
    {
        _categoryRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CategoryEntity>());

        var results = await _sut.GetAllCategoriesAsync();

        Assert.Empty(results);
    }
}
