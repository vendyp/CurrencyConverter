using CurrencyConverter.Application.Abstractions;

namespace CurrencyConverter.Infrastructure.Services;

internal class FrankfurterService : ICurrencyConverterProvider
{
    public Task<CurrencyConverterResponse> GetAllCurrencyAsync(CurrencyConverterRequest request,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<List<CurrencyRate>> GetAllAvailableCurrencyRatesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}