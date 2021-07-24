using Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Authorization
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if ((context.User.Identity != null && !context.User.Identity.IsAuthenticated) || context.User.Identity == null)
            {
                context.Fail();
                return;
            }

            var permissions = context.User.Claims.ToList();
            if (permissions.Count == 0)
            {
                context.Fail();
                return;
            }

            if (permissions.Any(x => x.Type == CustomClaimTypes.Permission
                                     && x.Value == requirement.Permission
                                     && x.Issuer == "LOCAL AUTHORITY"))
            {
                context.Succeed(requirement);
                return;
            }
            context.Fail();
        }
    }
}
