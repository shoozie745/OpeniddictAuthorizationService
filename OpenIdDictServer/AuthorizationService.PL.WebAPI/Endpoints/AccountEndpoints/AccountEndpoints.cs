using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using OpenIddict.Validation.AspNetCore;

namespace AuthorizationService.PL.WebAPI.Endpoints.AccountEndpoints;

public class AccountEndpoints
{
    public void RegisterApi(WebApplication app)
    {
        app.MapGet("~/api/account/get-claims", GetClaims).WithOpenApi();
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(401)]
    //[Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public async Task<IResult> GetClaims( 
        [FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var user = httpContextAccessor.HttpContext!.User;
        var claims = ((ClaimsIdentity)user.Identity!).Claims;
        var result = claims.Select(x => new { Type = x.Type, ValueType = x.ValueType, Value = x.Value });
        //Log.Information($"Current user {user.Identity.Name} have following climes {result}");
        return Results.Ok(result);
    }
}