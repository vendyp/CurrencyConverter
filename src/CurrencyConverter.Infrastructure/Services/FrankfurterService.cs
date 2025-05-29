using System.Text;
using System.Text.Json.Serialization;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using Microsoft.AspNetCore.WebUtilities;

// ReSharper disable InconsistentNaming

namespace CurrencyConverter.Infrastructure.Services;

internal class FrankfurterService : ICurrencyConverterProvider
{
    private readonly HttpClient _httpClient;

    public FrankfurterService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Frankfurter");
    }

    internal sealed class FrankfurterRates : FrankfurterBaseResponse
    {
        [JsonPropertyName("date")] public string? Date { get; set; }

        [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    internal sealed class FrankfurterPaginationRates : FrankfurterBaseResponse
    {
        [JsonPropertyName("start_date")] public string? StartDate { get; set; }

        [JsonPropertyName("end_date")] public string? EndDate { get; set; }

        [JsonPropertyName("rates")] public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; } = new();
    }

    internal class FrankfurterBaseResponse
    {
        [JsonPropertyName("amount")] public decimal? Amount { get; set; }

        [JsonPropertyName("base")] public string? Base { get; set; }
    }

    public async Task<CurrencyConverterResponse> GetAllCurrencyAsync(
        CurrencyConverterRequest request,
        CancellationToken cancellationToken)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("v1");

        var queryParams = new Dictionary<string, string>
        {
            { "base", "EUR" }
        };

        if (request.Base.HasValue)
        {
            queryParams["base"] = request.Base.Value.ToString();
        }

        if (request.Filters != null)
        {
            queryParams["symbols"] = string.Join(',', request.Filters.Select(e => e.ToString()));
        }

        var isPagination = false;

        if (request.StartDate.HasValue)
        {
            isPagination = true;

            sb.Append($"/{request.StartDate.Value:yyyy-MM-dd}..");

            if (request.EndDate.HasValue)
            {
                sb.Append($"{request.EndDate.Value:yyyy-MM-dd}");
            }
        }
        else
        {
            sb.Append("/latest");
        }

        var relativePath = QueryHelpers.AddQueryString(sb.ToString(), queryParams!);
        var response = await _httpClient.GetAsync(relativePath, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new CurrencyConverterResponse
            {
                IsSuccess = false,
                ErrorMessage = content
            };
        }

        var result = new CurrencyConverterResponse
        {
            IsSuccess = true,
        };

        if (!isPagination)
        {
            var contentObj = DefaultJsonSerializer.Deserialize<FrankfurterRates>(content);

            _ = CurrencyExtensions.TryParse(contentObj!.Base, out var baseCurrency);

            result.Base = baseCurrency;
            result.StartDate = DateTime.Parse(contentObj.Date!);
            result.Rates.Add(string.Empty, contentObj.Rates
                .Select(e => new CurrencyRate
                {
                    CurrencyId = e.Key,
                    Rate = e.Value
                }).ToList());
        }
        else
        {
            var contentObj = DefaultJsonSerializer.Deserialize<FrankfurterPaginationRates>(content);

            _ = CurrencyExtensions.TryParse(contentObj!.Base, out var baseCurrency);
            result.Base = baseCurrency;
            result.StartDate = DateTime.Parse(contentObj.StartDate!);
            result.EndDate = DateTime.Parse(contentObj.EndDate!);
            foreach (var item in contentObj.Rates)
            {
                result.Rates.Add(item.Key, item.Value.Select(e => new CurrencyRate
                {
                    CurrencyId = e.Key,
                    Rate = e.Value
                }).ToList());
            }
        }

        return result;
    }

    public Task<List<CurrencyRate>> GetAllAvailableCurrencyRatesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}