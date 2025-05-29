using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace CurrencyConverter.WebApi.Endpoints;

[Route("api/test-redis")]
public class TestRedis : EndpointBaseAsync.WithRequest<TestRedisRequest>.WithResult<string?>
{
    private readonly IDistributedCache _cache;

    public TestRedis(IDistributedCache cache)
    {
        _cache = cache;
    }

    public static string RelativePath => "/api/test-redis/{0}";

    [AllowAnonymous]
    [HttpGet("{key}")]
    public override async Task<string?> HandleAsync([FromRoute] TestRedisRequest request,
        CancellationToken cancellationToken = new CancellationToken())
    {
        return await _cache.GetStringAsync(request.Key, cancellationToken);
    }
}

public class TestRedisRequest
{
    [FromRoute(Name = "key")] public string Key { get; set; } = null!;
}