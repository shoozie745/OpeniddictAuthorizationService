using Microsoft.AspNetCore.Authorization;

namespace AuthorizationService.PL.WebAPI.Extensions.Builder.OpenIdDict;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string PermissionName { get; }
    public PermissionRequirement(string permissionName) => PermissionName = permissionName;
}