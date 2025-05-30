using CurrencyConverter.Domain.Enums;

namespace CurrencyConverter.Application;

public class CurrencyConverterManager
{
    public List<Enums.Currency> ExcludedCurrencies { get; } = [];

    public CurrencyConverterManager()
    {
        ExcludedCurrencies.Add(Enums.Currency.TRY);
        ExcludedCurrencies.Add(Enums.Currency.PLN);
        ExcludedCurrencies.Add(Enums.Currency.MXN);
    }
}