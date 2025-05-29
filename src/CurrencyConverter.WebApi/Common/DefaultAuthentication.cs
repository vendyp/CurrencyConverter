using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace CurrencyConverter.WebApi.Common;

internal static class DefaultAuthentication
{
    public static void AddDefaultAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        if (jwtSettings == null)
        {
            throw new InvalidOperationException("JwtSettings is missing");
        }

        #region validate JwtSettings

        if (string.IsNullOrWhiteSpace(jwtSettings.Key))
        {
            throw new InvalidOperationException("JwtSettings.Key is missing");
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.Issuer))
        {
            throw new InvalidOperationException("JwtSettings.Issuer is missing");
        }

        if (string.IsNullOrWhiteSpace(jwtSettings.Audience))
        {
            throw new InvalidOperationException("JwtSettings.Audience is missing");
        }

        #endregion

        var rawKey = Encoding.UTF8.GetBytes(jwtSettings.Key);
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(rawKey),
            ClockSkew = TimeSpan.Zero,
        };

        services.AddSingleton(jwtSettings);
        services
            .AddAuthentication(e =>
            {
                e.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                e.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(e =>
            {
                e.Audience = jwtSettings.Audience;
                e.TokenValidationParameters = tokenValidationParameters;
            });
    }
}