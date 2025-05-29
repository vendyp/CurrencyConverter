using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Infrastructure;

public static class ConfigureService
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var frankfurterTimeoutValue = configuration["FrankfurterSettings:TimeoutSettingInSecond"];
        var frankfurterTimeoutIsInt = int.TryParse(frankfurterTimeoutValue, out var frankfurterTimeout);
        if (!frankfurterTimeoutIsInt)
        {
            throw new InvalidOperationException("FrankfurterSettings:TimeoutSettingInSecond is required.");
        }

        var frankfurterBaseUrl = configuration["FrankfurterSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(frankfurterBaseUrl))
        {
            throw new InvalidOperationException("FrankfurterSettings:BaseUrl is required.");
        }

        services.AddHttpClient("Frankfurter", client =>
        {
            client.BaseAddress = new Uri(frankfurterBaseUrl);
            client.Timeout = TimeSpan.FromSeconds(frankfurterTimeout);
        });
        services.AddScoped<ICurrencyConverterProvider, FrankfurterService>();
    }
}