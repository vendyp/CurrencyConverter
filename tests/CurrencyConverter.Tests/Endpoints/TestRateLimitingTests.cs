using System.Net;
using CurrencyConverter.Application.Common;
using CurrencyConverter.WebApi.Endpoints;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Endpoints;

public class TestRateLimitingTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly ITestOutputHelper _outputHelper;
    private readonly HttpClient _client;

    public TestRateLimitingTests(DefaultWebApplicationFactory factory, ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestRateLimiting_On_TestAuthenticatedApi_Exceed_Threshold_Should_Return_TooManyRequests()
    {
        var response = await _client.GetAsync(TestGenerateToken.RelativePath);
        var content = await response.Content.ReadAsStringAsync();
        _outputHelper.WriteLine($"Login resp: {content}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var testGenerateTokenResponse = DefaultJsonSerializer.Deserialize<TestGenerateTokenResponse>(content);

        const int threshold = 50;
        for (var i = 1; i <= 51; i++)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, TestAuthenticatedApi.RelativePath);
            httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse!.Token}");
            response = await _client.SendAsync(httpRequest);

            if (i > 45)
            {
                _outputHelper.WriteLine($"Iteration: {i}, Status Code: {response.StatusCode}");
            }

            response.StatusCode.ShouldBe(i > threshold ? HttpStatusCode.TooManyRequests : HttpStatusCode.OK);
        }
    }
}