using TaskSystem.Api.Dtos;
using TaskSystem.Api.Services;

namespace TaskSystem.Api.Endpoints;

public static class CategoryEndpoints
{
    public static RouteGroupBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories")
            .WithTags("Categories");

        group.MapGet("/", GetCategories)
            .WithName("GetCategories")
            .WithSummary("List seeded task categories.")
            .Produces<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> GetCategories(ICategoryService categoryService, CancellationToken cancellationToken)
    {
        var categories = await categoryService.GetAllCategoriesAsync(cancellationToken);
        return Results.Ok(categories);
    }
}
