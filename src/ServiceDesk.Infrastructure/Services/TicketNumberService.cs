using Microsoft.EntityFrameworkCore;
using ServiceDesk.Application.Tickets;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Infrastructure.Persistence;

namespace ServiceDesk.Infrastructure.Services;

public sealed class TicketNumberService(AppDbContext db) : ITicketNumberService
{
    private static readonly char[] MonthLetters = ['A','B','C','D','E','F','G','H','I','J','K','L'];

    public async Task<string> NextAsync(int? teamId, DateTime utcNow, CancellationToken ct = default)
    {
        // Use app's local convention from architecture: ticket numbers are based on Manila/local business context in legacy format.
        // For simplicity, derive from UTC+8 to match legacy style.
        var local = utcNow.AddHours(8);
        var yy = local.Year % 100;
        var monthLetter = MonthLetters[Math.Clamp(local.Month - 1, 0, 11)];

        var teamCode = "MIS";
        if (teamId.HasValue)
        {
            var code = await db.Teams.AsNoTracking()
                .Where(t => t.Id == teamId.Value)
                .Select(t => t.Code)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(code))
            {
                teamCode = code.Trim().ToUpperInvariant();
            }
        }
        else
        {
            var mis = await db.Teams.AsNoTracking()
                .Where(t => t.Code.ToUpper() == "MIS")
                .Select(t => t.Code)
                .FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(mis)) teamCode = mis.Trim().ToUpperInvariant();
        }

        var scope = $"{teamCode}-{yy:00}{monthLetter}";

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        TicketNumberSequence? seq;
        try
        {
            // Lock row if it exists
            seq = await db.TicketNumberSequences
                .FromSqlRaw("SELECT * FROM `ticket_number_sequences` WHERE `scope_key` = {0} FOR UPDATE", scope)
                .AsTracking()
                .FirstOrDefaultAsync(ct);
        }
        catch
        {
            // Fallback for providers/scenarios without FOR UPDATE support in this path
            seq = await db.TicketNumberSequences.FirstOrDefaultAsync(x => x.ScopeKey == scope, ct);
        }

        if (seq is null)
        {
            seq = new TicketNumberSequence { ScopeKey = scope, LastValue = 0 };
            db.TicketNumberSequences.Add(seq);
        }

        seq.LastValue += 1;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return $"{scope}{seq.LastValue:000}";
    }
}
