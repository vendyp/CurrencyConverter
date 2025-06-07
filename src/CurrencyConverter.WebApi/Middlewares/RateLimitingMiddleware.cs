using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.WebApi.Services;

namespace CurrencyConverter.WebApi.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestContext = context.RequestServices.GetService<IRequestContext>();
        var rateLimiter = context.RequestServices.GetRequiredService<DefaultRateLimitingService>();

        if (requestContext is { Identity.IsAuthenticated: true })
        {
            var key = string.Format(Constants.RedisKey.RateLimitingKeyFormat,
                requestContext.Identity.UserId.ToString(),
                context.Request.Path);

            if (!await rateLimiter.IsRequestAllowedAsync(key))
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }
        }

        await _next(context);
    }
}