using System.Net;
using CurrencyConverter.WebApi.Endpoints;
using Shouldly;

namespace CurrencyConverter.Tests.Endpoints;

public class TestAuthenticatedApiTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TestAuthenticatedApiTests(DefaultWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestApi_Should_Return_Ok()
    {
        var response = await _client.GetAsync(TestAuthenticatedApi.RelativePath);
        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }
}