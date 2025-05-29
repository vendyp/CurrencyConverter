using System.Security.Claims;
using CurrencyConverter.Application.Abstractions;

namespace CurrencyConverter.WebApi.Common;

internal sealed class DefaultRequestContext : IRequestContext
{
    public DefaultRequestContext()
    {
        TraceId = string.Empty;
        DeviceModel = null;
        Identity = DefaultIdentityContext.Empty;
    }

    public DefaultRequestContext(HttpContext context) : this(context.TraceIdentifier,
        new DefaultIdentityContext(context.User),
        GetUserIpAddress(context),
        context.Request.Headers["user-agent"])
    {
    }

    public DefaultRequestContext(string traceId,
        IIdentityContext? identity = null,
        string? ipAddress = null,
        string? userAgent = null) : this()
    {
        TraceId = traceId;
        Identity = identity ?? DefaultIdentityContext.Empty;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public Guid RequestId { get; } = Guid.NewGuid();
    public string TraceId { get; }
    public string? IpAddress { get; }
    public string? UserAgent { get; }
    public string? DeviceModel { get; }
    public IIdentityContext Identity { get; }

    public static string GetUserIpAddress(HttpContext? context)
    {
        if (context is null)
        {
            return string.Empty;
        }

        var ipAddress = context.Connection.RemoteIpAddress?.ToString();

        if (!context.Request.Headers.TryGetValue("x-forwarded-for", out var forwardedFor))
            return ipAddress ?? string.Empty;

        var ipAddresses = forwardedFor.ToString().Split(",", StringSplitOptions.RemoveEmptyEntries);
        if (ipAddresses.Any())
            ipAddress = ipAddresses[0];

        return ipAddress ?? string.Empty;
    }

    public static IRequestContext Empty => new DefaultRequestContext();
}

internal sealed class DefaultIdentityContext : IIdentityContext
{
    private DefaultIdentityContext()
    {
        UserId = Guid.Empty;
    }

    public DefaultIdentityContext(Guid? id) : this()
    {
        if (id.HasValue)
        {
            UserId = id.Value;
        }
    }

    public DefaultIdentityContext(ClaimsPrincipal principal) : this()
    {
        if (principal.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return;
        }

        UserId = Guid.Parse(principal.Claims.FirstOrDefault(e => e.Type == Constants.ClaimNames.Identifier)!.Value);
    }

    public static IIdentityContext Empty => new DefaultIdentityContext();
    public Guid UserId { get; }
}