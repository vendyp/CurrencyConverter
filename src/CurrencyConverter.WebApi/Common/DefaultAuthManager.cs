using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.WebApi.Model;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyConverter.WebApi.Common;

public interface IAuthManager
{
    AuthTokenDto CreateToken(UserTaskModel userTaskModel);

    AuthTokenDto CreateToken(List<Claim> claims);
}

public sealed class AuthTokenDto
{
    public string? Token { get; set; }
    public long Expiry { get; set; }
}

internal sealed class DefaultAuthManager : IAuthManager
{
    private readonly JwtSettings _jwtSettings;
    private readonly IClockService _clockService;
    private readonly SigningCredentials _signingCredentials;

    public DefaultAuthManager(
        JwtSettings jwtSettings,
        IClockService clockService)
    {
        _jwtSettings = jwtSettings;
        _clockService = clockService;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key!)),
            SecurityAlgorithms.HmacSha256);
    }

    public AuthTokenDto CreateToken(UserTaskModel userTaskModel)
    {
        var createdDateTime = _clockService.GetCurrentDateTime();
        var expires = createdDateTime.Add(TimeSpan.FromDays(3));

        var claims = DefaultClaimService.ConstructClaims(userTaskModel);

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: createdDateTime,
            expires: expires,
            signingCredentials: _signingCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AuthTokenDto
        {
            Token = token,
            Expiry = new DateTimeOffset(expires).ToUnixTimeMilliseconds(),
        };
    }

    public AuthTokenDto CreateToken(List<Claim> claims)
    {
        var createdDateTime = _clockService.GetCurrentDateTime();
        var expires = createdDateTime.Add(TimeSpan.FromDays(3));

        var jwt = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: createdDateTime,
            expires: expires,
            signingCredentials: _signingCredentials
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new AuthTokenDto
        {
            Token = token,
            Expiry = new DateTimeOffset(expires).ToUnixTimeMilliseconds(),
        };
    }
}

internal sealed class JwtSettings
{
    public string? Key { get; set; }

    public string? Issuer { get; set; }

    public string? Audience { get; set; }
}