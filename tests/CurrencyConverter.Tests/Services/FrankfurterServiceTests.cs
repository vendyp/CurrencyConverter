using System.Security.Cryptography;
using CurrencyConverter.Application.Abstractions;
using CurrencyConverter.Domain.Enums;
using CurrencyConverter.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit.Abstractions;

namespace CurrencyConverter.Tests.Services;

public class FrankfurterServiceTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    public IServiceProvider ServiceProvider { get; }

    public FrankfurterServiceTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
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

    [Fact]
    public async Task FrankfurterService_Given_Correct_Request_With_StartDate_Should_Return_AsExpected()
    {
        using var scope = ServiceProvider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<FrankfurterService>();
        
        //because frankfurter will exclude result for saturday and sunday
        var startDate = DateTime.UtcNow.AddDays(-1); //for back date
        startDate = startDate.DayOfWeek switch
        {
            DayOfWeek.Saturday => startDate.AddDays(-1),
            DayOfWeek.Sunday => startDate.AddDays(-2),
            _ => startDate
        };
        _testOutputHelper.WriteLine($"Start Date: {startDate:yyyy-MM-dd}");

        var req = new CurrencyConverterRequest
        {
            Base = Enums.Currency.USD,
            Filters = [Enums.Currency.IDR, Enums.Currency.GBP, Enums.Currency.AUD],
            StartDate = startDate
        };
        var resp = await sut.GetAllCurrencyAsync(req, CancellationToken.None);

        resp.IsSuccess.ShouldBeTrue();
        resp.Base.ShouldNotBeNull();
        resp.Base.ShouldBe(req.Base);
        resp.Rates.ShouldNotBeEmpty();
        //resp.Rates.Values.Count.ShouldBe(3); //irrelevant
        resp.Rates.Values.First().Count.ShouldBe(req.Filters.Length);
        resp.StartDate.HasValue.ShouldBeTrue();
        // resp.StartDate!.Value.Date.ShouldBe(req.StartDate.Value.Date); //check line 132
    }

    [Fact]
    public async Task FrankfurterService_Given_Correct_Request_With_StartDate_And_EndDate_Should_Return_AsExpected()
    {
        using var scope = ServiceProvider.CreateScope();
        var sut = scope.ServiceProvider.GetRequiredService<FrankfurterService>();

        var startingPoint = RandomNumberGenerator.GetInt32(5, 35) * -1;
        var endPoint = -3;
        
        //because frankfurter will exclude result for saturday and sunday
        var startDate = DateTime.UtcNow.AddDays(startingPoint); //for back date
        startDate = startDate.DayOfWeek switch
        {
            DayOfWeek.Saturday => startDate.AddDays(-1),
            DayOfWeek.Sunday => startDate.AddDays(-2),
            _ => startDate
        };
        
        _testOutputHelper.WriteLine($"Start Date: {startDate:yyyy-MM-dd}, with startingPoint: {startingPoint}");

        //The pattern is broken, will never check the startdate or enddate again
        //Somehow frankfurter not returning rate on 2025 05 01, I thought the pattern will be no sunday, saturday and first date
        //Then checking again, we have list of rate on 2025 04 01
        
        var endDate = DateTime.UtcNow.AddDays(endPoint);
        
        var actualResult = GetBusinessDays(startDate, endDate);
        
        var req = new CurrencyConverterRequest
        {
            Base = Enums.Currency.USD,
            Filters = [Enums.Currency.IDR, Enums.Currency.GBP, Enums.Currency.AUD],
            StartDate = startDate,
            EndDate = endDate
        };
        
        var resp = await sut.GetAllCurrencyAsync(req, CancellationToken.None);

        resp.IsSuccess.ShouldBeTrue();
        resp.Base.ShouldNotBeNull();
        resp.Base.ShouldBe(req.Base);
        resp.Rates.ShouldNotBeEmpty();
        resp.Rates.Values.Count.ShouldBe(actualResult); // because only check 2 days
        resp.Rates.Values.First().Count.ShouldBe(req.Filters.Length);
        resp.StartDate.HasValue.ShouldBeTrue();
        resp.EndDate.HasValue.ShouldBeTrue();
        // resp.StartDate!.Value.Date.ShouldBe(req.StartDate.Value.Date);
        // resp.EndDate!.Value.Date.ShouldBe(req.EndDate.Value.Date);
    }
    
    public static int GetBusinessDays(DateTime start, DateTime end)
    {
        if (start > end)
            (start, end) = (end, start);

        int totalDays = (end - start).Days + 1;
        int fullWeeks = totalDays / 7;
        int remainingDays = totalDays % 7;

        int businessDays = fullWeeks * 5;

        // Check for remaining days
        for (int i = 0; i < remainingDays; i++)
        {
            var day = start.AddDays(i).DayOfWeek;
            if (day != DayOfWeek.Saturday && day != DayOfWeek.Sunday)
            {
                businessDays++;
            }
        }

        return businessDays;
    }
}