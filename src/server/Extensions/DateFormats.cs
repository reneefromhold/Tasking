namespace TaskSystem.Api.Extensions;

public static class DateFormats
{
    public const string StorageFormat = "yyyy-MM-ddTHH:mm:ssZ";

    public static string ToStorageString(DateTime value) =>
        value.ToUniversalTime().ToString(StorageFormat);
}
