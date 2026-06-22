using TaskSystem.Api.Dtos;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Extensions;

namespace TaskSystem.Api.Tests.Extensions;

public class ValidationExtensionsTests
{
    [Fact]
    public void CreateTaskRequest_RequiresTitle()
    {
        var request = new CreateTaskRequest { Title = "" };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Title", ex.Message);
    }

    [Fact]
    public void CreateTaskRequest_RejectsWhitespaceOnlyTitle()
    {
        var request = new CreateTaskRequest { Title = "   " };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Title", ex.Message);
    }

    [Fact]
    public void CreateTaskRequest_RejectsTitleOverMaxLength()
    {
        var request = new CreateTaskRequest { Title = new string('a', 31) };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Title", ex.Message);
    }

    [Fact]
    public void CreateTaskRequest_RejectsDescriptionOverMaxLength()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Description = new string('a', 501)
        };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Description", ex.Message);
    }

    [Fact]
    public void CreateTaskRequest_PassesWithValidPayload()
    {
        var request = new CreateTaskRequest
        {
            Title = "Valid title",
            Description = "Optional details"
        };

        var exception = Record.Exception(() => request.ValidateRequest());

        Assert.Null(exception);
    }

    [Fact]
    public void UpdateTaskRequest_RequiresTitle()
    {
        var request = new UpdateTaskRequest { Title = "" };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Title", ex.Message);
    }

    [Fact]
    public void UpdateTaskRequest_RejectsTitleOverMaxLength()
    {
        var request = new UpdateTaskRequest { Title = new string('a', 31) };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Title", ex.Message);
    }

    [Fact]
    public void CreateUserRequest_RejectsWhitespaceOnlyFirstName()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            FirstName = "   ",
            LastName = "Doe"
        };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("FirstName", ex.Message);
    }

    [Fact]
    public void CreateUserRequest_RejectsWhitespaceOnlyLastName()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            FirstName = "Jane",
            LastName = "   "
        };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("LastName", ex.Message);
    }

    [Fact]
    public void CreateUserRequest_TrimsValidFieldsBeforeValidation()
    {
        var request = new CreateUserRequest
        {
            Email = "  user@example.com  ",
            FirstName = "  Jane  ",
            LastName = "  Doe  "
        };

        var exception = Record.Exception(() => request.ValidateRequest());

        Assert.Null(exception);
        Assert.Equal("user@example.com", request.Email);
        Assert.Equal("Jane", request.FirstName);
        Assert.Equal("Doe", request.LastName);
    }

    [Fact]
    public void CreateUserRequest_RequiresEmailFirstNameAndLastName()
    {
        var request = new CreateUserRequest
        {
            Email = "",
            FirstName = "",
            LastName = ""
        };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Email", ex.Message);
        Assert.Contains("FirstName", ex.Message);
        Assert.Contains("LastName", ex.Message);
    }

    [Fact]
    public void CreateUserRequest_RejectsInvalidEmail()
    {
        var request = new CreateUserRequest
        {
            Email = "not-an-email",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var ex = Assert.Throws<ValidationAppException>(() => request.ValidateRequest());

        Assert.Contains("Email", ex.Message);
    }

    [Fact]
    public void CreateUserRequest_PassesWithValidPayload()
    {
        var request = new CreateUserRequest
        {
            Email = "user@example.com",
            FirstName = "Jane",
            LastName = "Doe"
        };

        var exception = Record.Exception(() => request.ValidateRequest());

        Assert.Null(exception);
    }
}
