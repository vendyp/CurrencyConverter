using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Infrastructure;
using CurrencyConverter.WebApi.Common;
using CurrencyConverter.WebApi.Jobs;
using CurrencyConverter.WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // still needs this in order to Ardalis.ApiEndpoints to work
builder.Services.AddSingleton<IAuthManager, DefaultAuthManager>();
builder.Services.AddSingleton<IClockService, DefaultClockService>();
builder.Services.AddDefaultAuthentication(builder.Configuration);
builder.Services.AddSingleton<DefaultContextAccessor>();
builder.Services.AddTransient(sp => sp.GetRequiredService<DefaultContextAccessor>().Context!);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString(CurrencyConverter.Domain.Constants.ConnectionStringName.RedisConnection);
});
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<FetchAllCurrencyBackgroundService>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.Use((ctx, next) =>
{
    ctx.RequestServices.GetRequiredService<DefaultContextAccessor>().Context = new DefaultRequestContext(ctx);

    return next();
});
app.MapControllers() // still needs this in order to Ardalis.ApiEndpoints to work
    .RequireAuthorization();

app.Run();