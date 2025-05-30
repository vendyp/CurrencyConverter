using System.Net;
using System.Text;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.WebApi.Endpoints;
using CurrencyConverter.WebApi.Endpoints.Currency;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Endpoints.Currency;

public class CurrencyConversionTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public CurrencyConversionTests(
        DefaultWebApplicationFactory factory,
        ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CurrencyConversion_Given_Invalid_Request_Should_Return_BadRequest()
    {
        var response = await _client.GetAsync(TestGenerateToken.RelativePath);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

#if DEBUG
        _output.WriteLine($"TestGenerateToken content: {content}");
#endif

        var testGenerateTokenResponse = DefaultJsonSerializer.Deserialize<TestGenerateTokenResponse>(content);


        var request = new CurrencyConversionRequest
        {
            BaseCurrency = nameof(Enums.Currency.IDR),
            Amount = 10000m,
            ToCurrency = nameof(Enums.Currency.PLN) // excluded currency
        };

#if DEBUG
        _output.WriteLine($"TestGenerateToken content: {content}");
#endif

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, CurrencyConversion.RelativePath);
        httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse!.Token}");
        httpRequest.Content = new StringContent(
            DefaultJsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");
        response = await _client.SendAsync(httpRequest);
        response.IsSuccessStatusCode.ShouldBeFalse();
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
}