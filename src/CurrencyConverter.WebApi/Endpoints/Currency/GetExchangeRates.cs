using Ardalis.ApiEndpoints;
using CurrencyConverter.Application;
using CurrencyConverter.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Endpoints.Currency;

[Route("api/currency/rates")]
public class GetExchangeRates : EndpointBaseAsync
    .WithRequest<GetExchangeRatesRequest>
    .WithActionResult<GetExchangeRatesResponse>
{
    private readonly CurrencyConverterManager _converterManager;

    public GetExchangeRates(CurrencyConverterManager converterManager)
    {
        _converterManager = converterManager;
    }

    public static string RelativePath => "/api/currency/rates";

    [HttpGet]
    public override async Task<ActionResult<GetExchangeRatesResponse>> HandleAsync(
        [FromQuery] GetExchangeRatesRequest request,
        CancellationToken cancellationToken = new())
    {
        var validator = new GetExchangeRatesRequestValidation(_converterManager);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest();
        }

        throw new NotImplementedException();
    }
}

public class GetExchangeRatesRequest
{
    [FromQuery(Name = "from")] public DateTime? From { get; set; }
    [FromQuery(Name = "to")] public DateTime? To { get; set; }
    [FromQuery(Name = "base")] public string? BaseCurrency { get; set; }
}

public class GetExchangeRatesRequestValidation : AbstractValidator<GetExchangeRatesRequest>
{
    public GetExchangeRatesRequestValidation(CurrencyConverterManager converterManager)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.BaseCurrency)
            .NotNull()
            .NotEmpty()
            .Must(CurrencyExtensions.IsDefined!)
            .Custom((curr, context) =>
            {
                if (CurrencyExtensions.TryParse(curr, out var value))
                {
                    if (converterManager.ExcludedCurrencies.Any(e => value == e))
                    {
                        context.AddFailure("Invalid currency");
                    }
                }
                else
                {
                    context.AddFailure("Invalid currency");
                }
            });

        When(e => e.From is not null, () =>
        {
            RuleFor(e => e.From)
                .Custom((e, context) =>
                {
                    var ts = DateTime.UtcNow - e!.Value;
                    if (ts.TotalDays > 30)
                    {
                        context.AddFailure("Maximum date range should be between 30 days");
                    }
                });

            When(e => e.To is not null, () =>
            {
                RuleFor(e => e.From)
                    .LessThan(e => e.To);

                RuleFor(e => e)
                    .Custom((e, context) =>
                    {
                        var ts = e.To!.Value - e.From!.Value;
                        if (ts.TotalDays > 30)
                        {
                            context.AddFailure("Maximum date range should be between 30 days");
                        }
                    });
            });
        });
    }
}

public class GetExchangeRatesResponse
{
}