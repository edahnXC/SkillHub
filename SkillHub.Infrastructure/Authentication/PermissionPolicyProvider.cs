using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

using SkillHub.Application.Features.Identity.Constants;

namespace SkillHub.Infrastructure.Authentication;

public class PermissionPolicyProvider
    : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider
        _fallbackPolicyProvider;

    public PermissionPolicyProvider(
        IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider =
            new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(
        string policyName)
    {
        // If the policy name starts with "Permission:",
        // dynamically create a policy with PermissionRequirement
        if (policyName.StartsWith(
                PermissionConstants.PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            var permissionName = policyName
                [PermissionConstants.PolicyPrefix.Length..];

            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(
                    new PermissionRequirement(permissionName))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // Fall back to the default provider for
        // role-based and other standard policies
        return _fallbackPolicyProvider
            .GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider
            .GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider
            .GetFallbackPolicyAsync();
    }
}
