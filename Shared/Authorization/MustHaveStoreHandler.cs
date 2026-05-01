using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Shared.Authorization;

public class MustHaveStoreHandler : AuthorizationHandler<MustHaveStoreRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, MustHaveStoreRequirement requirement)
    {
        var storeIdClaim = context.User.FindFirstValue("store_id");

        if (!string.IsNullOrWhiteSpace(storeIdClaim) && Guid.TryParse(storeIdClaim, out _))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
