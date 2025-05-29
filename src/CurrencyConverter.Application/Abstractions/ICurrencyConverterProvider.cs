using CurrencyConverter.Domain.Enums;

namespace CurrencyConverter.Application.Abstractions;

public interface ICurrencyConverterProvider
{
    Task<CurrencyConverterResponse> GetAllCurrencyAsync(
        CurrencyConverterRequest request,
        CancellationToken cancellationToken);

    Task<List<CurrencyRate>> GetAllAvailableCurrencyRatesAsync(CancellationToken cancellationToken);
}

public sealed class CurrencyConverterRequest
{
    public Enums.Currency? Base { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
    
    public Enums.Currency[]? Filters { get; set; }
}

public sealed class CurrencyConverterResponse : BaseCurrencyConverterContract
{
    public Enums.Currency? Base { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public Dictionary<string, List<CurrencyRate>> Rates { get; set; } = [];
}

public sealed class CurrencyRate
{
    public string? CurrencyId { get; set; }
    public Enums.Currency Currency { get; set; } = Enums.Currency.Undefined;
    public bool IsCurrencyUndefined => Currency == Enums.Currency.Undefined;
    public decimal Rate { get; set; } = 0m;
}

public abstract class BaseCurrencyConverterContract
{
    public bool IsSuccess { get; set; } = false;
    public string? ErrorMessage { get; set; }
}