using CurrencyConverter.WebApi;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace CurrencyConverter.Tests;

public class DefaultWebApplicationFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7.0")
        .WithCleanUp(true)
        .Build();


    private readonly IContainer _seqContainer = new ContainerBuilder()
        .WithName(Guid.NewGuid().ToString("D"))
        .WithImage("datalust/seq:latest")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(e =>
        {
            e.ClearProviders();
            e.AddConsole();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IDistributedCache>();
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = _redisContainer.GetConnectionString();
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        //await _seqContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _redisContainer.StopAsync();
        //await _seqContainer.StopAsync();
    }

    public async Task<string?> GetStringAsync(string key)
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(_redisContainer.GetConnectionString());
        var db = redis.GetDatabase(); // Get the value
        RedisValue value = await db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }
}