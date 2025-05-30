using System.Net;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.WebApi.Endpoints;
using CurrencyConverter.WebApi.Endpoints.Currency;
using Microsoft.AspNetCore.WebUtilities;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Endpoints.Currency;

public class GetExchangeRatesTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public GetExchangeRatesTests(
        DefaultWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetExchangeRates_Given_Invalid_Request_Should_Return_BadRequest()
    {
        var response = await _client.GetAsync(TestGenerateToken.RelativePath);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

#if DEBUG
        _output.WriteLine($"TestGenerateToken content: {content}");
#endif

        var testGenerateTokenResponse = DefaultJsonSerializer.Deserialize<TestGenerateTokenResponse>(content);

        var queryParams = new Dictionary<string, string>
        {
            { "base", nameof(Enums.Currency.PLN) } // excluded currency
        };
        var relativePath = QueryHelpers.AddQueryString(GetExchangeRates.RelativePath, queryParams!);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativePath);
        httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse!.Token}");
        response = await _client.SendAsync(httpRequest);
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetExchangeRates_Given_Invalid_Request_Exceed_30_TotalDays_Should_Return_BadRequest()
    {
        var response = await _client.GetAsync(TestGenerateToken.RelativePath);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

#if DEBUG
        _output.WriteLine($"TestGenerateToken content: {content}");
#endif

        var testGenerateTokenResponse = DefaultJsonSerializer.Deserialize<TestGenerateTokenResponse>(content);

        var queryParams = new Dictionary<string, string>
        {
            { "base", nameof(Enums.Currency.IDR) },
            { "from", DateTime.UtcNow.AddDays(-33).ToString("yyyy-MM-dd") } //exceed maximum 30 days
        };
        var relativePath = QueryHelpers.AddQueryString(GetExchangeRates.RelativePath, queryParams!);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativePath);
        httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse!.Token}");
        response = await _client.SendAsync(httpRequest);
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}