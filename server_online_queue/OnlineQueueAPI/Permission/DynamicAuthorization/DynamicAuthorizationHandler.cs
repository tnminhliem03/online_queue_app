using Microsoft.AspNetCore.Authorization;
using OnlineQueueAPI.DL;
using OnlineQueueAPI.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace OnlineQueueAPI.Permission
{
    public class DynamicAuthorizationHandler : AuthorizationHandler<DynamicAuthorizationRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrganizationDL _organizationDL;

        public DynamicAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IOrganizationDL organizationDL)
        {
            _httpContextAccessor = httpContextAccessor;
            _organizationDL = organizationDL;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            DynamicAuthorizationRequirement requirement)
        {
            var userIdStr = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return;
            var userId = Guid.Parse(userIdStr);

            var userRole = context.User.FindFirstValue(ClaimTypes.Role);

            if (_httpContextAccessor.HttpContext == null) return;
            var request = _httpContextAccessor.HttpContext.Request;
            var path = request.Path.Value!.ToLower();
            var method = request.Method.ToUpper();

            var matchedPermission = ApiPermissions.Permissions.FirstOrDefault(p =>
                method == p.Key.Method && MatchPath(path, p.Key.Path)
            );

            if (matchedPermission.Key == default) return;

            var allowedRoles = matchedPermission.Value;

            if (userRole == UserRole.Admin.ToString() && allowedRoles.Contains("Admin"))
            {
                context.Succeed(requirement);
                return;
            }

            if (allowedRoles.Contains("Manager") || allowedRoles.Contains("Customer"))
            {
                var organizationId = await GetOrganizationIdFromRequest(request);

                if (organizationId.HasValue)
                {
                    var role = await _organizationDL.GetUserOrganizationRoleWithOrganization(userId, organizationId.Value);

                    if ((allowedRoles.Contains("Manager") && role == Role.Manager) ||
                        (allowedRoles.Contains("Customer") && role == Role.Customer))
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
            }
        }

        private bool MatchPath(string actualPath, string pattern)
        {
            var actualSegments = actualPath.Trim('/').Split('/');
            var patternSegments = pattern.Trim('/').Split('/');

            if (actualSegments.Length != patternSegments.Length) return false;

            for (int i = 0; i < actualSegments.Length; i++)
            {
                if (patternSegments[i].StartsWith("{") && patternSegments[i].EndsWith("}"))
                    continue; 
                if (!string.Equals(actualSegments[i], patternSegments[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private async Task<Guid?> GetOrganizationIdFromRequest(HttpRequest request)
        {
            var possibleSources = new List<Func<Task<Guid?>>>
            {
                () => Task.FromResult(
                    request.Query.TryGetValue("organizationId", out var orgIdStr)
                    && Guid.TryParse(orgIdStr, out var orgId) ? orgId : (Guid?)null
                ),

                async () => request.Query.TryGetValue("serviceId", out var serviceIdStr)
                    && Guid.TryParse(serviceIdStr, out var serviceId)
                    ? await _organizationDL.GetOrganizationIdFromServiceId(serviceId)
                    : null,

                async () => request.RouteValues.TryGetValue("id", out var idValue)
                                && Guid.TryParse(idValue?.ToString(), out var entityId)
                    ? await GetOrganizationIdFromId(entityId, request.Path.Value)
                    : null,

                async () => request.Method == HttpMethods.Post ? await GetOrganizationIdFromBody(request) : null
            };

            foreach (var source in possibleSources)
            {
                var result = await source();
                if (result.HasValue) return result;
            }

            return null;
        }

        private async Task<Guid?> GetOrganizationIdFromId(Guid id, string? path)
        {
            if (string.IsNullOrEmpty(path)) return null;

            var lookup = new Dictionary<string, Func<Guid, Task<Guid?>>>
            {
                ["/organizations/"] = async (guid) => await Task.FromResult<Guid?>(guid),
                ["/services/"] = _organizationDL.GetOrganizationIdFromServiceId,
                ["/queues/"] = _organizationDL.GetOrganizationIdFromQueueId,
                ["/appointments/"] = _organizationDL.GetOrganizationIdFromAppointmentId
            };

            foreach (var key in lookup.Keys)
            {
                if (path.Contains(key)) return await lookup[key](id);
            }

            return null;
        }

        private async Task<Guid?> GetOrganizationIdFromBody(HttpRequest request)
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                if (!string.IsNullOrEmpty(body))
                {
                    var jsonDocument = JsonDocument.Parse(body);

                    var lookup = new Dictionary<string, Func<Guid, Task<Guid?>>>
                    {
                        ["organizationId"] = async (id) => await Task.FromResult<Guid?>(id),
                        ["serviceId"] = _organizationDL.GetOrganizationIdFromServiceId,
                        ["queueId"] = _organizationDL.GetOrganizationIdFromQueueId,
                        ["appointmentId"] = _organizationDL.GetOrganizationIdFromAppointmentId
                    };

                    foreach (var key in lookup.Keys)
                    {
                        if (jsonDocument.RootElement.TryGetProperty(key, out var element)
                            && Guid.TryParse(element.GetString(), out Guid entityId))
                        {
                            return await lookup[key](entityId);
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"[DEBUG] ERROR JSON parsing: {ex.Message}");
            }

            return null;
        }
    }
}
