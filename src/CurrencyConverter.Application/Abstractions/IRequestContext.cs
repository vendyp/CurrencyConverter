namespace CurrencyConverter.Application.Abstractions;

public interface IRequestContext
{
    Guid RequestId { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    string? DeviceModel { get; }

    IIdentityContext? Identity { get; }
}

public interface IIdentityContext
{
    bool IsAuthenticated { get; }
    
    Guid UserId { get; }
}