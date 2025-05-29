using Ardalis.ApiEndpoints;
using CurrencyConverter.WebApi.Common;
using CurrencyConverter.WebApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.WebApi.Endpoints;

[Route("api/test-generate-token")]
public class TestGenerateToken : EndpointBaseSync.WithoutRequest.WithActionResult<TestGenerateTokenResponse>
{
    private readonly IAuthManager _authManager;

    public TestGenerateToken(IAuthManager authManager)
    {
        _authManager = authManager;
    }

    public static string RelativePath => "/api/test-generate-token";

    [AllowAnonymous]
    [HttpGet]
    public override ActionResult<TestGenerateTokenResponse> Handle()
    {
        var tokenResult = _authManager.CreateToken(new UserTaskModel
        {
            UserId = Guid.NewGuid()
        });

        return new TestGenerateTokenResponse
        {
            Token = tokenResult.Token,
            Expiry = tokenResult.Expiry,
        };
    }
}

public class TestGenerateTokenResponse
{
    public string? Token { get; set; }

    public long Expiry { get; set; }

    public string Scheme { get; set; } = "Bearer";
}