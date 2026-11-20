namespace ServiceDesk.Application.Exceptions;

/// <summary>
/// A request broke a business rule, such as a duplicate name or a status move the workflow
/// doesn't allow. The message is shown to the user as written, so it must be a full,
/// friendly sentence that says what to do next.
/// </summary>
public class BusinessRuleException(string message) : Exception(message);
