namespace ServiceDesk.Application.Exceptions;

/// <summary>
/// The record a request asked for doesn't exist, or the current user isn't allowed to know it
/// exists. Ticket lookups outside the user's access scope throw this too, so a guessed ticket id
/// reveals nothing.
/// </summary>
public sealed class NotFoundException(string entityName, object key)
    : Exception($"We couldn't find that {entityName.ToLowerInvariant()}. It may have been removed, or the link may be out of date.")
{
    /// <summary>Friendly name of the record type, for example "Ticket".</summary>
    public string EntityName { get; } = entityName;

    /// <summary>The key that was looked up. Logged, never shown to the user.</summary>
    public object Key { get; } = key;
}
