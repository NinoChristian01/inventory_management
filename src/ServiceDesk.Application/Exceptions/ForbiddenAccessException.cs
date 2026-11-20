namespace ServiceDesk.Application.Exceptions;

/// <summary>
/// The user is signed in but lacks the permission an action needs.
/// </summary>
public sealed class ForbiddenAccessException(string message = "You don't have permission to do that. If you need it, ask an administrator.")
    : Exception(message);
