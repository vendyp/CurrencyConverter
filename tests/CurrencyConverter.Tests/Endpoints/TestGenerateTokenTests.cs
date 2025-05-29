using System.Net;
using CurrencyConverter.Application.Common;
using CurrencyConverter.WebApi.Endpoints;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Endpoints;

public class TestGenerateTokenTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly ITestOutputHelper _output;
    private readonly HttpClient _client;

    public TestGenerateTokenTests(DefaultWebApplicationFactory factory, ITestOutputHelper output)
    {
        _output = output;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestGenerateToken_Should_Do_AsExpected()
    {
        var response = await _client.GetAsync(TestGenerateToken.RelativePath);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();

        _output.WriteLine($"TestGenerateToken content: {content}");

        var testGenerateTokenResponse =
            DefaultJsonSerializer.Deserialize<TestGenerateTokenResponse>(content);

        testGenerateTokenResponse.ShouldNotBeNull();

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, TestAuthenticatedApi.RelativePath);
        httpRequest.Headers.Add("Authorization", $"Bearer {testGenerateTokenResponse.Token}");

        response = await _client.SendAsync(httpRequest);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        content = await response.Content.ReadAsStringAsync();
        _output.WriteLine($"TestAuthenticatedApi content: {content}");

        var testAuthenticatedApiResponse =
            DefaultJsonSerializer.Deserialize<TestAuthenticatedApiResponse>(content);
        
        testAuthenticatedApiResponse.ShouldNotBeNull();
        testAuthenticatedApiResponse.UserId.ShouldNotBe(Guid.Empty);
    }
}