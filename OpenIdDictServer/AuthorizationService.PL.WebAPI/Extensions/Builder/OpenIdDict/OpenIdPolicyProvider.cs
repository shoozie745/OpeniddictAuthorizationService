using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AuthorizationService.PL.WebAPI.Extensions.Builder.OpenIdDict;

public class OpenIdPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    public OpenIdPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) => _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policyExists = await base.GetPolicyAsync(policyName);
        if (policyExists != null)
        {
            return policyExists;
        }

        policyExists = new AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement(policyName)).Build();
        _options.AddPolicy(policyName, policyExists);
        return policyExists;
    }
}

