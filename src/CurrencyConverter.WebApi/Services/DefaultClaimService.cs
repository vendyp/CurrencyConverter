using System.Security.Claims;
using CurrencyConverter.WebApi.Model;

namespace CurrencyConverter.WebApi.Common;

internal static class DefaultClaimService
{
    public static List<Claim> ConstructClaims(UserTaskModel userTaskModel)
    {
        var claims = new List<Claim>
        {
            new(Constants.ClaimNames.Identifier, userTaskModel.UserId!.Value.ToString()),
        };

        return claims;
    }
}