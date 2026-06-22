using System.Text.Json;
using TaskSystem.Api.Data;
using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Tests.Dtos;

public class TaskDtoSerializationTests
{
    [Fact]
    public void CreateTaskRequest_DeserializesFromCamelCaseJson()
    {
        const string json = """
            {
              "title": "Ship MVP",
              "description": "Finish backend",
              "categoryId": "33333333-3333-3333-3333-333333333331",
              "assignee": "11111111-1111-1111-1111-111111111111",
              "dueDate": "2026-12-31T23:59:59Z"
            }
            """;

        var request = JsonSerializer.Deserialize<CreateTaskRequest>(json, JsonTestOptions.Web);

        Assert.NotNull(request);
        Assert.Equal("Ship MVP", request.Title);
        Assert.Equal("Finish backend", request.Description);
        Assert.Equal(SeedIds.Epic1CategoryId, request.CategoryId);
        Assert.Equal(SeedIds.DemoUserId, request.Assignee);
        Assert.Equal("2026-12-31T23:59:59Z", request.DueDate);
    }

    [Fact]
    public void CreateTaskRequest_DeserializesMinimalBody()
    {
        const string json = """{ "title": "Minimal task" }""";

        var request = JsonSerializer.Deserialize<CreateTaskRequest>(json, JsonTestOptions.Web);

        Assert.NotNull(request);
        Assert.Equal("Minimal task", request.Title);
        Assert.Null(request.CategoryId);
        Assert.Null(request.Description);
        Assert.Null(request.Assignee);
        Assert.Null(request.DueDate);
    }

    [Fact]
    public void UpdateTaskRequest_DeserializesFromCamelCaseJson()
    {
        const string json = """
            {
              "title": "Updated title",
              "description": "Updated details",
              "categoryId": "33333333-3333-3333-3333-333333333332",
              "assignee": "11111111-1111-1111-1111-111111111111",
              "dueDate": "2027-01-01T00:00:00Z"
            }
            """;

        var request = JsonSerializer.Deserialize<UpdateTaskRequest>(json, JsonTestOptions.Web);

        Assert.NotNull(request);
        Assert.Equal("Updated title", request.Title);
        Assert.Equal("Updated details", request.Description);
        Assert.Equal(SeedIds.Epic2CategoryId, request.CategoryId);
        Assert.Equal(SeedIds.DemoUserId, request.Assignee);
        Assert.Equal("2027-01-01T00:00:00Z", request.DueDate);
    }

    [Fact]
    public void TaskResponse_SerializesExpectedCamelCasePropertyNames()
    {
        var response = new TaskResponse(
            Id: "task-1",
            CategoryId: SeedIds.Epic1CategoryId,
            CategoryName: "Epic 1",
            Title: "Ship MVP",
            Description: "Details",
            Creator: SeedIds.DemoUserId,
            Assignee: SeedIds.DemoUserId,
            CreateDate: "2026-01-01T00:00:00Z",
            DueDate: "2026-12-31T23:59:59Z");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.Equal("task-1", root.GetProperty("id").GetString());
        Assert.Equal(SeedIds.Epic1CategoryId, root.GetProperty("categoryId").GetString());
        Assert.Equal("Epic 1", root.GetProperty("categoryName").GetString());
        Assert.Equal("Ship MVP", root.GetProperty("title").GetString());
        Assert.Equal("Details", root.GetProperty("description").GetString());
        Assert.Equal(SeedIds.DemoUserId, root.GetProperty("creator").GetString());
        Assert.Equal(SeedIds.DemoUserId, root.GetProperty("assignee").GetString());
        Assert.Equal("2026-01-01T00:00:00Z", root.GetProperty("createDate").GetString());
        Assert.Equal("2026-12-31T23:59:59Z", root.GetProperty("dueDate").GetString());
    }

    [Fact]
    public void TaskResponse_SerializesNullOptionalFields()
    {
        var response = new TaskResponse(
            Id: "task-1",
            CategoryId: null,
            CategoryName: null,
            Title: "Minimal",
            Description: null,
            Creator: SeedIds.DemoUserId,
            Assignee: null,
            CreateDate: "2026-01-01T00:00:00Z",
            DueDate: null);

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Null, root.GetProperty("categoryId").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("categoryName").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("description").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("assignee").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("dueDate").ValueKind);
    }

    [Fact]
    public void TaskResponseList_SerializesAsJsonArray()
    {
        var tasks = new List<TaskResponse>
        {
            new("task-1", null, null, "First", null, SeedIds.DemoUserId, null, "2026-01-01T00:00:00Z", null),
            new("task-2", null, null, "Second", null, SeedIds.DemoUserId, null, "2026-01-02T00:00:00Z", null)
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(tasks, JsonTestOptions.Web));

        Assert.Equal(JsonValueKind.Array, doc.RootElement.ValueKind);
        Assert.Equal(2, doc.RootElement.GetArrayLength());
        Assert.Equal("First", doc.RootElement[0].GetProperty("title").GetString());
        Assert.Equal("Second", doc.RootElement[1].GetProperty("title").GetString());
    }
}
