using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace OnlineQueueAPI.Permission
{
    public class OwnerAuthorizationHandler : AuthorizationHandler<OwnerRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnerAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OwnerRequirement requirement)
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            var userId = userIdClaim?.Value;

            if (_httpContextAccessor.HttpContext == null || !_httpContextAccessor.HttpContext.Request.RouteValues.ContainsKey("id"))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var isAdmin = context.User.HasClaim(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            if (isAdmin)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            var ownerId = _httpContextAccessor.HttpContext.Request.RouteValues["id"]?.ToString();
            if (userId == ownerId)
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
