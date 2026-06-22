namespace TaskSystem.Api.Extensions;

public static class StringNormalization
{
    /// <summary>
    /// Trims the value, or returns null when the input is null/whitespace-only.
    /// </summary>
    public static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
