using Ardalis.ApiEndpoints;
using CurrencyConverter.Application;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Endpoints.Currency;

[Route("api/currency/conversion")]
public class CurrencyConversion : EndpointBaseAsync
    .WithRequest<CurrencyConversionRequest>
    .WithActionResult<CurrencyConversionResponse>
{
    public static string RelativePath => "/api/currency/conversion";

    private readonly IDistributedCache _distributedCache;
    private readonly ICurrencyConverterProvider _converterProvider;
    private readonly CurrencyConverterManager _currencyConverterManager;

    public CurrencyConversion(
        IDistributedCache distributedCache,
        ICurrencyConverterProvider converterProvider,
        CurrencyConverterManager currencyConverterManager)
    {
        _distributedCache = distributedCache;
        _converterProvider = converterProvider;
        _currencyConverterManager = currencyConverterManager;
    }

    [HttpPost]
    public override async Task<ActionResult<CurrencyConversionResponse>> HandleAsync(
        [FromBody] CurrencyConversionRequest request,
        CancellationToken cancellationToken = new())
    {
        var validator = new CurrencyConversionRequestValidation(_currencyConverterManager);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest();
        }

        List<CurrencyRate> rates;
        var baseCurrency = CurrencyExtensions.Parse(request.BaseCurrency);
        var currencyKey = string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, baseCurrency);
        var toCurrency = CurrencyExtensions.Parse(request.ToCurrency);
        var cachedString = await _distributedCache.GetStringAsync(currencyKey,
            cancellationToken);
        if (string.IsNullOrEmpty(cachedString))
        {
            var results = await _converterProvider.GetAllCurrencyAsync(new CurrencyConverterRequest
            {
                Base = baseCurrency,
            }, cancellationToken);

            rates = results.Rates.SelectMany(e => e.Value).ToList();

            await _distributedCache.SetStringAsync(currencyKey,
                DefaultJsonSerializer.Serialize(results.Rates),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23)
                },
                cancellationToken);
        }
        else
        {
            rates = DefaultJsonSerializer.Deserialize<List<CurrencyRate>>(cachedString)!;
        }

        var convertedAmount = Math.Round(request.Amount!.Value * rates.First(e => e.Currency == toCurrency).Rate, 2);

        return new CurrencyConversionResponse
        {
            Amount = request.Amount!.Value,
            ToAmount = convertedAmount
        };
    }
}

public class CurrencyConversionRequest
{
    public string? BaseCurrency { get; set; }
    public decimal? Amount { get; set; }
    public string? ToCurrency { get; set; }
}

public class CurrencyConversionRequestValidation : AbstractValidator<CurrencyConversionRequest>
{
    public CurrencyConversionRequestValidation(CurrencyConverterManager currencyConverterManager)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.BaseCurrency)
            .NotNull()
            .NotEmpty()
            .Must(CurrencyExtensions.IsDefined!).Custom((curr, context) =>
            {
                if (CurrencyExtensions.TryParse(curr, out var value))
                {
                    if (currencyConverterManager.ExcludedCurrencies.Any(e => value == e))
                    {
                        context.AddFailure("Invalid currency");
                    }
                }
                else
                {
                    context.AddFailure("Invalid currency");
                }
            });

        RuleFor(e => e.Amount)
            .NotNull()
            .GreaterThan(0);

        RuleFor(e => e.ToCurrency)
            .NotNull()
            .NotEmpty()
            .Must(CurrencyExtensions.IsDefined!).Custom((curr, context) =>
            {
                if (CurrencyExtensions.TryParse(curr, out var value))
                {
                    if (currencyConverterManager.ExcludedCurrencies.Any(e => value == e))
                    {
                        context.AddFailure("Invalid currency");
                    }
                }
                else
                {
                    context.AddFailure("Invalid currency");
                }
            });
    }
}

public class CurrencyConversionResponse
{
    public decimal? Amount { get; set; }
    public decimal? ToAmount { get; set; }
}