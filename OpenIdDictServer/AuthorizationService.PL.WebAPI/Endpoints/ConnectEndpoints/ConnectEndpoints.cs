using System.Security.Claims;
using AuthorizationServer.BL.Services.AccountManagerService;
using AuthorizationService.DAL.Models;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Server.AspNetCore.OpenIddictServerAspNetCoreDefaults;

namespace AuthorizationService.PL.WebAPI.Endpoints.ConnectEndpoints;

public class ConnectEndpoints
{
    public void RegisterConnectEndpoints(WebApplication app)
    {
        //app.MapGet("~/api/account/get-claims", GetClaims).WithOpenApi();
    }
    
}