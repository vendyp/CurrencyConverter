using System.Net;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.WebApi.Endpoints;
using CurrencyConverter.WebApi.Endpoints.Currency;
using Microsoft.AspNetCore.WebUtilities;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Endpoints.Currency;

public class GetLatestExchangeRatesTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public GetLatestExchangeRatesTests(
        DefaultWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestApi_Should_Return_Ok()
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
            { "curr", nameof(Enums.Currency.IDR) }
        };
        var relativePath = QueryHelpers.AddQueryString(GetLatestExchangeRates.RelativePath, queryParams!);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, relativePath);
        httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse!.Token}");
        response = await _client.SendAsync(httpRequest);
        content = await response.Content.ReadAsStringAsync();

#if DEBUG
        _output.WriteLine($"GetLatestExchangeRates content: {content}");
#endif

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var key = string.Format(WebApi.Constants.RedisKey.LatestExchangeRatesKeyFormat,
            nameof(Enums.Currency.IDR));
#if DEBUG
        _output.WriteLine($"Key content: {key}");
#endif
        
        var url = string.Format(TestRedis.RelativePath, key);

#if DEBUG
        _output.WriteLine($"Url content: {url}");
#endif
        
        response = await _client.GetAsync(string.Format(TestRedis.RelativePath, key));
        content = await response.Content.ReadAsStringAsync();
        response.IsSuccessStatusCode.ShouldBeTrue();
        content.ShouldNotBeNullOrWhiteSpace();
    }
}