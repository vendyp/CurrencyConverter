using System.Net;
using CurrencyConverter.WebApi.Endpoints;
using Shouldly;

namespace CurrencyConverter.Tests.Endpoints;

public class TestApiTests : IClassFixture<DefaultWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TestApiTests(DefaultWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestApi_Should_Return_Ok()
    {
        var response = await _client.GetAsync(TestApi.Route);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}