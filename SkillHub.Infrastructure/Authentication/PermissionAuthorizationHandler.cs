using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Authentication;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly ApplicationDbContext _context;

    public PermissionAuthorizationHandler(
        ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User
            .FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null ||
            !long.TryParse(userIdClaim.Value, out var userId))
        {
            return;
        }

        // Check if the user has the required permission
        // through their roles → role permissions → permission
        var hasPermission = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .AnyAsync(rp =>
                rp.Permission.Name == requirement.Permission);

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
    }
}
