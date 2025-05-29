using Ardalis.ApiEndpoints;
using CurrencyConverter.Application.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Endpoints;

[Route("api/test-authenticated")]
public class TestAuthenticatedApi : EndpointBaseSync.WithoutRequest.WithActionResult<TestAuthenticatedApiResponse>
{
    private readonly IRequestContext _requestContext;

    public TestAuthenticatedApi(IRequestContext requestContext)
    {
        _requestContext = requestContext;
    }

    public static string RelativePath => "/api/test-authenticated";

    [HttpGet]
    public override ActionResult<TestAuthenticatedApiResponse> Handle()
    {
        return Ok(new TestAuthenticatedApiResponse
        {
            UserId = _requestContext.Identity!.UserId
        });
    }
}

public class TestAuthenticatedApiResponse
{
    public Guid UserId { get; set; }
}