using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Endpoints;

[Route("api/test")]
public class TestApi : EndpointBaseSync.WithoutRequest.WithActionResult
{
    public static string Route => "/api/test";

    [AllowAnonymous]
    [HttpGet]
    public override ActionResult Handle()
    {
        return Ok();
    }
}