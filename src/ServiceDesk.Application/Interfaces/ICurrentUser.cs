using System.Security.Claims;

namespace ServiceDesk.Application.Interfaces;

public interface ICurrentUser
{
    long? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
    ClaimsPrincipal? Principal { get; }
}
