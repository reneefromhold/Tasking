using System.Text.Json;
using TaskSystem.Api.Data;
using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Tests.Dtos;

public class CategoryDtoSerializationTests
{
    [Fact]
    public void CategoryResponse_SerializesExpectedCamelCasePropertyNames()
    {
        var response = new CategoryResponse(SeedIds.Epic1CategoryId, "Epic 1");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.Equal(SeedIds.Epic1CategoryId, root.GetProperty("id").GetString());
        Assert.Equal("Epic 1", root.GetProperty("category").GetString());
    }

    [Fact]
    public void CategoryResponseList_SerializesAsJsonArray()
    {
        var categories = new List<CategoryResponse>
        {
            new(SeedIds.Epic1CategoryId, "Epic 1"),
            new(SeedIds.Epic2CategoryId, "Epic 2")
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(categories, JsonTestOptions.Web));

        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal("Epic 1", doc.RootElement[0].GetProperty("category").GetString());
        Assert.Equal("Epic 2", doc.RootElement[1].GetProperty("category").GetString());
    }
}
