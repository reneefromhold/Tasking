using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using TaskSystem.Api.Dtos;
using TaskSystem.Api.Exceptions;
using TaskSystem.Api.Middleware;

namespace TaskSystem.Api.Tests.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Theory]
    [InlineData(typeof(ValidationAppException), "Title is required.", StatusCodes.Status400BadRequest)]
    [InlineData(typeof(NotFoundException), "Task not found.", StatusCodes.Status404NotFound)]
    [InlineData(typeof(ConflictException), "Email already exists.", StatusCodes.Status409Conflict)]
    [InlineData(typeof(UnauthorizedAppException), "Missing header.", StatusCodes.Status401Unauthorized)]
    public async Task InvokeAsync_MapsAppExceptionsToStatusAndErrorResponse(
        Type exceptionType,
        string message,
        int expectedStatusCode)
    {
        var context = CreateHttpContext();
        var exception = (Exception)Activator.CreateInstance(exceptionType, message)!;
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(expectedStatusCode, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);

        var error = await ReadErrorResponseAsync(context);
        Assert.Equal(message, error.Message);
        Assert.Null(error.Detail);
    }

    [Fact]
    public async Task InvokeAsync_MapsValidationExceptionTo400()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ValidationException("Invalid value."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var error = await ReadErrorResponseAsync(context);
        Assert.Equal("Invalid value.", error.Message);
    }

    [Fact]
    public async Task InvokeAsync_MapsUnknownExceptionTo500WithGenericMessage()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Sensitive internals."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var error = await ReadErrorResponseAsync(context);
        Assert.Equal("An unexpected error occurred.", error.Message);
    }

    [Fact]
    public async Task InvokeAsync_ErrorResponse_SerializesMessagePropertyInCamelCase()
    {
        var context = CreateHttpContext();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Missing resource."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await JsonDocument.ParseAsync(context.Response.Body);

        Assert.True(doc.RootElement.TryGetProperty("message", out var message));
        Assert.Equal("Missing resource.", message.GetString());
        Assert.False(doc.RootElement.TryGetProperty("Message", out _));
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<ErrorResponse> ReadErrorResponseAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var error = await JsonSerializer.DeserializeAsync<ErrorResponse>(
            context.Response.Body,
            JsonTestOptions.Web);

        Assert.NotNull(error);
        return error;
    }
}
