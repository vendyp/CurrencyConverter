using System.Text.Json;

namespace CurrencyConverter.Application.Common;

public static class DefaultJsonSerializer
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions => new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static string Serialize(object request)
    {
        return JsonSerializer.Serialize(request, DefaultJsonSerializerOptions);
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
    }
}