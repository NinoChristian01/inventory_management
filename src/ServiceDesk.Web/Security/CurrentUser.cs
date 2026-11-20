using System.Security.Claims;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Application.Security;

namespace ServiceDesk.Web.Security;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public ClaimsPrincipal? Principal => User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public long? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return long.TryParse(value, out var id) ? id : null;
        }
    }

    public string? UserName => User?.Identity?.Name;

    public bool IsInRole(string role) => User?.IsInRole(role) == true;

    public bool HasPermission(string permission) =>
        User?.Claims.Any(c => c.Type == Permissions.ClaimType && c.Value == permission) == true;
}
