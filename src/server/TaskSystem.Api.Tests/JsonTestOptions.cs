using System.Text.Json;

namespace TaskSystem.Api.Tests;

/// <summary>
/// Matches ASP.NET Core default web JSON options (camelCase property names).
/// </summary>
internal static class JsonTestOptions
{
    public static JsonSerializerOptions Web { get; } = new(JsonSerializerDefaults.Web);
}
