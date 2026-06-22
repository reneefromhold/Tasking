using System.Text.Json;
using TaskSystem.Api.Data;
using TaskSystem.Api.Dtos;

namespace TaskSystem.Api.Tests.Dtos;

public class UserDtoSerializationTests
{
    [Fact]
    public void CreateUserRequest_DeserializesFromCamelCaseJson()
    {
        const string json = """
            {
              "email": "new@example.com",
              "firstName": "Jane",
              "lastName": "Doe"
            }
            """;

        var request = JsonSerializer.Deserialize<CreateUserRequest>(json, JsonTestOptions.Web);

        Assert.NotNull(request);
        Assert.Equal("new@example.com", request.Email);
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Doe", request.LastName);
    }

    [Fact]
    public void UserResponse_SerializesExpectedCamelCasePropertyNames()
    {
        var response = new UserResponse(
            Id: SeedIds.DemoUserId,
            Email: "demo@tasksystem.com",
            FirstName: "Demo",
            LastName: "User");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.Equal(SeedIds.DemoUserId, root.GetProperty("id").GetString());
        Assert.Equal("demo@tasksystem.com", root.GetProperty("email").GetString());
        Assert.Equal("Demo", root.GetProperty("firstName").GetString());
        Assert.Equal("User", root.GetProperty("lastName").GetString());
    }

    [Fact]
    public void UserStatusResponse_SerializesWhenUserExists()
    {
        var response = new UserStatusResponse(Exists: true, Status: "active", Id: SeedIds.DemoUserId);

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.True(root.GetProperty("exists").GetBoolean());
        Assert.Equal("active", root.GetProperty("status").GetString());
        Assert.Equal(SeedIds.DemoUserId, root.GetProperty("id").GetString());
    }

    [Fact]
    public void UserStatusResponse_SerializesWhenUserNotFound()
    {
        var response = new UserStatusResponse(Exists: false, Status: "not_found");

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(response, JsonTestOptions.Web));
        var root = doc.RootElement;

        Assert.False(root.GetProperty("exists").GetBoolean());
        Assert.Equal("not_found", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("id").ValueKind);
    }

    [Fact]
    public void UserSummaryResponseList_SerializesExpectedShape()
    {
        var users = new List<UserSummaryResponse>
        {
            new(SeedIds.DemoUserId, "demo@tasksystem.com", "Demo", "User")
        };

        using var doc = JsonDocument.Parse(JsonSerializer.Serialize(users, JsonTestOptions.Web));
        var user = doc.RootElement[0];

        Assert.Equal(SeedIds.DemoUserId, user.GetProperty("id").GetString());
        Assert.Equal("demo@tasksystem.com", user.GetProperty("email").GetString());
        Assert.Equal("Demo", user.GetProperty("firstName").GetString());
        Assert.Equal("User", user.GetProperty("lastName").GetString());
    }
}
