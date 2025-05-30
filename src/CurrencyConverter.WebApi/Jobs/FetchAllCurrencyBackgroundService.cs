using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.WebApi.Common;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Jobs;

public class FetchAllCurrencyBackgroundService : BackgroundService, IBackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FetchAllCurrencyBackgroundService> _logger;

    public FetchAllCurrencyBackgroundService(IServiceProvider serviceProvider,
        ILogger<FetchAllCurrencyBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FetchAllCurrencyBackgroundService background job starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var currencyConverterProvider = scope.ServiceProvider.GetRequiredService<ICurrencyConverterProvider>();
                var cacheService = scope.ServiceProvider.GetRequiredService<IDistributedCache>();
                var clockService = scope.ServiceProvider.GetRequiredService<IClockService>();

                foreach (var item in CurrencyExtensions.GetValues())
                {
                    if (item == Enums.Currency.Undefined)
                    {
                        continue;
                    }

                    var latestExchangeRatesKeyFormat =
                        string.Format(Constants.RedisKey.LatestExchangeRatesKeyFormat, item.ToString());
                    var latestExchangeKeyFormatCache =
                        await cacheService.GetStringAsync(latestExchangeRatesKeyFormat, stoppingToken);
                    if (string.IsNullOrWhiteSpace(latestExchangeKeyFormatCache))
                    {
                        var response = await currencyConverterProvider.GetAllCurrencyAsync(new CurrencyConverterRequest
                        {
                            Base = item
                        }, stoppingToken);

                        if (response.IsSuccess)
                        {
                            var rates = response.Rates.SelectMany(e => e.Value).ToList();
                            await cacheService.SetStringAsync(latestExchangeRatesKeyFormat,
                                DefaultJsonSerializer.Serialize(rates),
                                new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23)
                                },
                                stoppingToken);
                        }

                        _logger.LogInformation("FetchAllCurrencyBackgroundService Get and Set: {Currency}", item);
                    }

                    var historyExchangeRatesKeyFormat =
                        string.Format(Constants.RedisKey.HistoryOf3MonthsFromNowExchangeRatesKeyFormat,
                            item.ToString());
                    var historyExchangeRatesKeyCache =
                        await cacheService.GetStringAsync(historyExchangeRatesKeyFormat, stoppingToken);
                    if (string.IsNullOrWhiteSpace(historyExchangeRatesKeyCache))
                    {
                        var response = await currencyConverterProvider.GetAllCurrencyAsync(new CurrencyConverterRequest
                        {
                            Base = item,
                            StartDate = clockService.GetCurrentDateTime().AddMonths(-3)
                        }, stoppingToken);

                        if (response.IsSuccess)
                        {
                            await cacheService.SetStringAsync(historyExchangeRatesKeyFormat,
                                DefaultJsonSerializer.Serialize(response.Rates),
                                new DistributedCacheEntryOptions
                                {
                                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23)
                                },
                                stoppingToken);
                        }
                    }
                }
            }

            await Task.Delay(TimeSpan.FromHours(23), stoppingToken);
        }

        _logger.LogInformation("FetchAllCurrencyBackgroundService background job stopping.");
    }
}