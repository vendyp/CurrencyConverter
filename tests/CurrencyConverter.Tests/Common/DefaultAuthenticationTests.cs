using CurrencyConverter.WebApi.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CurrencyConverter.Tests.Common;

public class DefaultAuthenticationTests
{
    [Fact]
    public void DefaultAuthentication_AddDefaultAuthentication_Should_ThrowException()
    {
        //arrange
        var services = new ServiceCollection();
        IConfiguration configuration = new ConfigurationBuilder()
            .Build();

        //act & assert
        Should.Throw<InvalidOperationException>(() => { services.AddDefaultAuthentication(configuration); });
    }

    [Fact]
    public void DefaultAuthentication_AddDefaultAuthentication_Given_Correct_JwtSettings_Should_Not_ThrowException()
    {
        //arrange
        var services = new ServiceCollection();

        var inMemorySettings = new Dictionary<string, string>
        {
            { "JwtSettings:Issuer", "test-issuer" },
            { "JwtSettings:Audience", "test-audience" },
            { "JwtSettings:Key", "test-key-which-is-long-enough" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        //act & assert
        Should.NotThrow(() => { services.AddDefaultAuthentication(configuration); });
    }
}