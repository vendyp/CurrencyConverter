using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Middlewares;

public class CustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();

        var logger = loggerFactory.CreateLogger<CustomExceptionHandler>();

        logger.LogError(exception, "Exception occured on path: {Path}", httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Title = "An unexpected error occurred, internal server error.",
            Detail = exception.Message,
            Status = StatusCodes.Status500InternalServerError,
            Instance = httpContext.Request.Path
        };
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        httpContext.Response.ContentType = "application/problem+json";

        var result = Results.Problem(problemDetails);

        await result.ExecuteAsync(httpContext);

        return true;
    }
}