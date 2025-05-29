using CurrencyConverter.Application.Abstractions;

namespace CurrencyConverter.WebApi.Services;

public sealed class DefaultClockService : IClockService
{
    public DateTime GetMachineDateTime() => GetCurrentDateTime().AddHours(7);

    public DateTime GetCurrentDateTime() => DateTime.UtcNow;
}