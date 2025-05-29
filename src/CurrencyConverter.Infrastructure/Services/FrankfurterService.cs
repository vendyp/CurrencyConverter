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

    internal sealed class FrankfurterRates
    {
        [JsonPropertyName("amount")] public decimal? Amount { get; set; }
        [JsonPropertyName("base")] public string? Base { get; set; }
        [JsonPropertyName("date")] public string? Date { get; set; }
        [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    internal sealed class FrankfurterPaginationRates
    {
        [JsonPropertyName("amount")] public string? Base { get; set; }

        [JsonPropertyName("start_date")] public string? StartDate { get; set; }

        [JsonPropertyName("end_date")] public string? EndDate { get; set; }

        [JsonPropertyName("rates")] public Dictionary<string, decimal> Rates { get; set; } = new();
    }

    internal class FrankfurterBaseResponse
    {
        [JsonPropertyName("amount")] public int? Amount { get; set; }

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

        if (request.StartDate is null && request.EndDate is null)
        {
            sb.Append("/latest");
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

            var contentObj = DefaultJsonSerializer.Deserialize<FrankfurterRates>(content);

            _ = CurrencyExtensions.TryParse(contentObj!.Base, out var baseCurrency);

            var result = new CurrencyConverterResponse
            {
                IsSuccess = true,
                Base = baseCurrency,
                StartDate = DateTime.Parse(contentObj!.Date!),
                EndDate = null,
            };

            result.Rates.Add(string.Empty, contentObj!.Rates
                .Select(e => new CurrencyRate
                {
                    CurrencyId = e.Key,
                    Rate = e.Value
                }).ToList());

            return result;
        }

        throw new NotImplementedException();
    }

    public Task<List<CurrencyRate>> GetAllAvailableCurrencyRatesAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}