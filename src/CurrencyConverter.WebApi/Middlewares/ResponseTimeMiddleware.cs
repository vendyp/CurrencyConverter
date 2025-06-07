using System.Diagnostics;

namespace CurrencyConverter.WebApi.Middlewares;

public class ResponseTimeMiddleware
{
    private readonly RequestDelegate _next;
    private const int ThresholdMilliseconds = 200;

    public ResponseTimeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await _next(context);

        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > ThresholdMilliseconds)
        {
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<ResponseTimeMiddleware>();

            var method = context.Request.Method;
            var path = context.Request.Path;
            var statusCode = context.Response.StatusCode;

            logger.LogWarning("Slow response: {method} {path} took {totalMilliseconds}ms (Status: {statusCode})",
                method, path, stopwatch.ElapsedMilliseconds, statusCode);
        }
    }
}