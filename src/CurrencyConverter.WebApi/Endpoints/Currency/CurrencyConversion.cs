using Ardalis.ApiEndpoints;
using CurrencyConverter.Application;
using CurrencyConverter.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Endpoints.Currency;

[Route("api/currency/conversion")]
public class CurrencyConversion : EndpointBaseAsync
    .WithRequest<CurrencyConversionRequest>
    .WithActionResult<CurrencyConversionResponse>
{
    public static string RelativePath => "/api/currency/conversion";
    
    private readonly CurrencyConverterManager _currencyConverterManager;

    public CurrencyConversion(CurrencyConverterManager currencyConverterManager)
    {
        _currencyConverterManager = currencyConverterManager;
    }

    [HttpPost]
    public override async Task<ActionResult<CurrencyConversionResponse>> HandleAsync(
        [FromBody] CurrencyConversionRequest request,
        CancellationToken cancellationToken = new())
    {
        var validator = new CurrencyConversionRequestValidation(_currencyConverterManager);
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest();
        }

        throw new NotImplementedException();
    }
}

public class CurrencyConversionRequest
{
    public string? BaseCurrency { get; set; }
    public decimal? Amount { get; set; }
    public string? ToCurrency { get; set; }
}

public class CurrencyConversionRequestValidation : AbstractValidator<CurrencyConversionRequest>
{
    public CurrencyConversionRequestValidation(CurrencyConverterManager currencyConverterManager)
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(e => e.BaseCurrency)
            .NotNull()
            .NotEmpty()
            .Must(CurrencyExtensions.IsDefined!).Custom((curr, context) =>
            {
                if (CurrencyExtensions.TryParse(curr, out var value))
                {
                    if (currencyConverterManager.ExcludedCurrencies.Any(e => value == e))
                    {
                        context.AddFailure("Invalid currency");
                    }
                }
                else
                {
                    context.AddFailure("Invalid currency");
                }
            });

        RuleFor(e => e.Amount)
            .NotNull()
            .GreaterThan(0);

        RuleFor(e => e.ToCurrency)
            .NotNull()
            .NotEmpty()
            .Must(CurrencyExtensions.IsDefined!).Custom((curr, context) =>
            {
                if (CurrencyExtensions.TryParse(curr, out var value))
                {
                    if (currencyConverterManager.ExcludedCurrencies.Any(e => value == e))
                    {
                        context.AddFailure("Invalid currency");
                    }
                }
                else
                {
                    context.AddFailure("Invalid currency");
                }
            });
    }
}

public class CurrencyConversionResponse
{
    public decimal? BaseAmount { get; set; }
    public decimal? Amount { get; set; }
    public decimal? ToAmount { get; set; }
}