using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyConverter.WebApi.Common;
using CurrencyConverter.WebApi.Model;
using CurrencyConverter.WebApi.Services;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

namespace CurrencyConverter.Tests.Common;

public class DefaultAuthManagerTests
{
    [Fact]
    public void DefaultAuthManager_Given_Correct_UserTaskModel_CreateToken_Should_Return_AsExpected()
    {
        var jwtSetting = new JwtSettings
        {
            Key = "GYADTWAFDATYWFIFHIUFHWIAHDKAJWHIUAHEIUAWHIUAHJSKDAFWA",
            Audience = "test",
            Issuer = "test",
        };

        var sut = new DefaultAuthManager(jwtSetting, new DefaultClockService());

        var userTaskModel = new UserTaskModel
        {
            UserId = Guid.NewGuid()
        };

        var result = sut.CreateToken(userTaskModel);
        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.Token.Split('.').Length.ShouldBe(3);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSetting.Key!);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = jwtSetting.Audience,
            ValidIssuer = jwtSetting.Issuer
        };

        Should.NotThrow(() => tokenHandler.ValidateToken(result.Token!, parameters, out _));
    }

    [Fact]
    public void DefaultAuthManager_Given_Correct_Claims_CreateToken_Should_Return_AsExpected()
    {
        var jwtSetting = new JwtSettings
        {
            Key = "GYADTWAFDATYWFIFHIUFHWIAHDKAJWHIUAHEIUAWHIUAHJSKDAFWA",
            Audience = "test",
            Issuer = "test",
        };

        var sut = new DefaultAuthManager(jwtSetting, new DefaultClockService());

        var claims = new List<Claim>
        {
            new(WebApi.Constants.ClaimNames.Identifier, Guid.NewGuid().ToString()),
            new(WebApi.Constants.ClaimNames.Name, Guid.NewGuid().ToString()),
        };

        var result = sut.CreateToken(claims);
        result.Token.ShouldNotBeNullOrWhiteSpace();
        result.Token.Split('.').Length.ShouldBe(3);

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(jwtSetting.Key!);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ClockSkew = TimeSpan.Zero,
            ValidAudience = jwtSetting.Audience,
            ValidIssuer = jwtSetting.Issuer
        };

        Should.NotThrow(() => tokenHandler.ValidateToken(result.Token!, parameters, out _));
    }
}