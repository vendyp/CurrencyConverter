using Ardalis.ApiEndpoints;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Endpoints.Currency;

[Route("api/currency/rates")]
public class GetLatestExchangeRates : EndpointBaseAsync
    .WithRequest<GetLatestExchangeRatesRequest>
    .WithActionResult<GetLatestExchangeRatesResponse>
{
    private readonly IDistributedCache _distributedCache;
    private readonly ICurrencyConverterProvider _converterProvider;

    public GetLatestExchangeRates(
        IDistributedCache distributedCache,
        ICurrencyConverterProvider converterProvider)
    {
        _distributedCache = distributedCache;
        _converterProvider = converterProvider;
    }

    public static string RelativePath => "/api/currency/rates/{0}";

    [HttpGet("{Currency}")]
    public override async Task<ActionResult<GetLatestExchangeRatesResponse>> HandleAsync(
        [FromRoute] GetLatestExchangeRatesRequest request,
        CancellationToken cancellationToken = new())
    {
        var validator = new GetLatestExchangeRatesRequestValidation();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest();
        }

        var cachedString = await _distributedCache.GetStringAsync(
            string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, request.Currency),
            cancellationToken);
        if (!string.IsNullOrEmpty(cachedString))
        {
            return new GetLatestExchangeRatesResponse
            {
                Rates = DefaultJsonSerializer.Deserialize<List<CurrencyRate>>(cachedString)!
            };
        }

        var currency = CurrencyExtensions.Parse(request.Currency);

        var results = await _converterProvider.GetAllCurrencyAsync(new CurrencyConverterRequest
        {
            Base = currency
        }, cancellationToken);

        var rates = results.Rates.SelectMany(e => e.Value).ToList();

        await _distributedCache.SetStringAsync(
            string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, request.Currency),
            DefaultJsonSerializer.Serialize(results.Rates),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23)
            },
            cancellationToken);

        return new GetLatestExchangeRatesResponse
        {
            Rates = rates
        };
    }
}

public class GetLatestExchangeRatesRequest
{
    [FromRoute(Name = "Currency")] public string Currency { get; set; } = null!;
}

public class GetLatestExchangeRatesRequestValidation : AbstractValidator<GetLatestExchangeRatesRequest>
{
    public GetLatestExchangeRatesRequestValidation()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.Currency)
            .NotNull()
            .NotEmpty()
            .Must(ValidCurrency);
    }

    private static bool ValidCurrency(string currency)
    {
        return CurrencyExtensions.IsDefined(currency);
    }
}

public class GetLatestExchangeRatesResponse
{
    public List<CurrencyRate> Rates { get; set; } = [];
}