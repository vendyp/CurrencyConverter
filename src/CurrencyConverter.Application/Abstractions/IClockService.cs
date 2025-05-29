namespace CurrencyConverter.Application.Abstractions;

public interface IClockService
{
    DateTime GetMachineDateTime();

    DateTime GetCurrentDateTime();
}