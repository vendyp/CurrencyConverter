using Ardalis.ApiEndpoints;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Endpoints.Currency;

[Route("api/currency/rates")]
public class GetLatestExchangeRates : EndpointBaseAsync
    .WithRequest<GetLatestExchangeRatesRequest>
    .WithResult<GetLatestExchangeRatesResponse>
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

    public static string RelativePath => "/api/currency/rates";

    [HttpGet]
    public override async Task<GetLatestExchangeRatesResponse> HandleAsync(
        GetLatestExchangeRatesRequest request,
        CancellationToken cancellationToken = new())
    {
        var cachedString = await _distributedCache.GetStringAsync(
            string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, request.Currency!.ToString()),
            cancellationToken);
        if (!string.IsNullOrEmpty(cachedString))
        {
            return new GetLatestExchangeRatesResponse
            {
                Rates = DefaultJsonSerializer.Deserialize<List<CurrencyRate>>(cachedString)!
            };
        }

        var results = await _converterProvider.GetAllCurrencyAsync(new CurrencyConverterRequest
        {
            Base = request.Currency
        }, cancellationToken);

        var rates = results.Rates.SelectMany(e => e.Value).ToList();

        await _distributedCache.SetStringAsync(
            string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, request.Currency!.ToString()),
            DefaultJsonSerializer.Serialize(rates),
            cancellationToken);

        return new GetLatestExchangeRatesResponse
        {
            Rates = rates
        };
    }
}

public class GetLatestExchangeRatesRequest
{
    [FromQuery(Name = "curr")] public Enums.Currency? Currency { get; set; } = Enums.Currency.EUR;
}

public class GetLatestExchangeRatesResponse
{
    public List<CurrencyRate> Rates { get; set; } = [];
}