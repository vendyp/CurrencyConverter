using CurrencyConverter.Application;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Application.Common;
using CurrencyConverter.Infrastructure;
using CurrencyConverter.WebApi.Common;
using CurrencyConverter.WebApi.Jobs;
using CurrencyConverter.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); // still needs this in order to Ardalis.ApiEndpoints to work
builder.Services.AddSingleton<IAuthManager, DefaultAuthManager>();
builder.Services.AddSingleton<IClockService, DefaultClockService>();
builder.Services.AddDefaultAuthentication(builder.Configuration);
builder.Services.AddSingleton<DefaultContextAccessor>();
builder.Services.AddTransient(sp => sp.GetRequiredService<DefaultContextAccessor>().Context!);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString(CurrencyConverter.Domain.Constants.ConnectionStringName
            .RedisConnection);
});
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<CurrencyConverterManager>();

builder.Services.AddHostedService<FetchAllCurrencyBackgroundService>();

builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler(err =>
{
    err.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Title = "Unexpected Error",
            Status = 500,
            Detail = "Something went wrong. Please try again later.",
            Instance = context.Request.Path
        };

        await context.Response.WriteAsJsonAsync(problem, DefaultJsonSerializer.DefaultJsonSerializerOptions);
    });
});

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