namespace ServiceDesk.Application.Tickets;

public interface ITicketNumberService
{
    Task<string> NextAsync(int? teamId, DateTime utcNow, CancellationToken ct = default);
}
