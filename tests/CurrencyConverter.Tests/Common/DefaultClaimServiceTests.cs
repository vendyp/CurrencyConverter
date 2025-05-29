using CurrencyConverter.WebApi.Common;
using CurrencyConverter.WebApi.Model;
using Shouldly;

namespace CurrencyConverter.Tests.Common;

public class DefaultClaimServiceTests
{
    [Fact]
    public void DefaultClaimService_ConstructClaims_Should_Do_AsExpected()
    {
        //arrange
        var dto = new UserTaskModel
        {
            UserId = Guid.NewGuid(),
        };

        //sut && test
        var claims = DefaultClaimService.ConstructClaims(dto);
        claims.Count.ShouldBePositive();
        var identifier = claims.FirstOrDefault(e => e.Type == WebApi.Constants.ClaimNames.Identifier);
        identifier.ShouldNotBeNull();
        identifier.Value.ShouldBe(dto.UserId.ToString());

        var name = claims.FirstOrDefault(e => e.Type == WebApi.Constants.ClaimNames.Identifier);
        name.ShouldNotBeNull();
        name.Value.ShouldBe(dto.UserId.ToString());
    }
}