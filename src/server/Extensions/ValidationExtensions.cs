using System.ComponentModel.DataAnnotations;
using System.Reflection;
using TaskSystem.Api.Exceptions;

namespace TaskSystem.Api.Extensions;

public static class ValidationExtensions
{
    public static void ValidateRequest<T>(this T request)
    {
        TrimStringProperties(request!);

        var context = new ValidationContext(request!);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(request!, context, results, validateAllProperties: true))
        {
            var message = string.Join(" ", results.Select(r => r.ErrorMessage));
            throw new ValidationAppException(message);
        }
    }

    private static void TrimStringProperties(object request)
    {
        foreach (var property in request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.PropertyType != typeof(string) || !property.CanWrite || !property.CanRead)
            {
                continue;
            }

            var value = (string?)property.GetValue(request);
            if (value is not null)
            {
                property.SetValue(request, value.Trim());
            }
        }
    }
}
