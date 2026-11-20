namespace ServiceDesk.Application.Exceptions;

/// <summary>
/// Someone else saved the same record after the current user loaded it. The stale edit is
/// refused instead of silently overwriting the newer version.
/// </summary>
public sealed class ConcurrencyConflictException(string entityName)
    : Exception($"Someone else changed this {entityName.ToLowerInvariant()} while you were editing. Reload the page to see the latest version, then make your change again.");
