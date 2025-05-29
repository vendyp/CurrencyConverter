using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace CurrencyConverter.Tests.Services;

public class FrankfurterServiceTests
{
    public IServiceProvider ServiceProvider { get; }

    public FrankfurterServiceTests()
    {
        var services = new ServiceCollection();
        services.AddHttpClient("Frankfurter", client =>
        {
            client.BaseAddress = new Uri("https://api.frankfurter.dev");
            client.Timeout = TimeSpan.FromSeconds(10);
        });
        services.AddScoped<FrankfurterService>();

        ServiceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task
        FrankfurterService_Given_Correct_Request_To_Get_Latest_With_Default_Base_Should_Return_AsExpected()
    {
        using var scope = ServiceProvider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<FrankfurterService>();
        var resp = await sut.GetAllCurrencyAsync(new CurrencyConverterRequest(), CancellationToken.None);
        resp.IsSuccess.ShouldBeTrue();
        resp.Base.ShouldNotBeNull();
        resp.Base.ShouldBe(Enums.Currency.EUR);
        resp.Rates.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task FrankfurterService_Given_Correct_Request_To_Get_Latest_With_Base_As_USD_Should_Return_AsExpected()
    {
        using var scope = ServiceProvider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<FrankfurterService>();
        var resp = await sut.GetAllCurrencyAsync(new CurrencyConverterRequest
        {
            Base = Enums.Currency.USD
        }, CancellationToken.None);
        resp.IsSuccess.ShouldBeTrue();
        resp.Base.ShouldNotBeNull();
        resp.Base.ShouldBe(Enums.Currency.USD);
        resp.Rates.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task FrankfurterService_Given_Correct_Request_With_Filters_Should_Return_AsExpected()
    {
        using var scope = ServiceProvider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<FrankfurterService>();
        var req = new CurrencyConverterRequest
        {
            Base = Enums.Currency.USD,
            Filters = [Enums.Currency.IDR, Enums.Currency.GBP, Enums.Currency.AUD]
        };
        var resp = await sut.GetAllCurrencyAsync(req, CancellationToken.None);

        resp.IsSuccess.ShouldBeTrue();
        resp.Base.ShouldNotBeNull();
        resp.Base.ShouldBe(req.Base);
        resp.Rates.ShouldNotBeEmpty();
        resp.Rates.Values.Count.ShouldBe(1);
        resp.Rates.Values.First().Count.ShouldBe(req.Filters.Length);
    }
}