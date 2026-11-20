using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.UnitTests.Web;

public sealed class DisplayHelpersTests
{
    [Theory]
    [InlineData(TicketStatusCode.New, "New", "sd-status-new")]
    [InlineData(TicketStatusCode.Open, "Open", "sd-status-open")]
    [InlineData(TicketStatusCode.InProgress, "In Progress", "sd-status-in-progress")]
    [InlineData(TicketStatusCode.PendingRequester, "Pending Requester", "sd-status-pending-requester")]
    [InlineData(TicketStatusCode.PendingThirdParty, "Pending Third Party", "sd-status-pending-third-party")]
    [InlineData(TicketStatusCode.OnHold, "On Hold", "sd-status-on-hold")]
    [InlineData(TicketStatusCode.Reopened, "Reopened", "sd-status-reopened")]
    [InlineData(TicketStatusCode.Resolved, "Resolved", "sd-status-resolved")]
    [InlineData(TicketStatusCode.Closed, "Closed", "sd-status-closed")]
    [InlineData(TicketStatusCode.Canceled, "Canceled", "sd-status-canceled")]
    public void Each_status_has_the_README_label_and_its_own_css_class(TicketStatusCode code, string label, string css)
    {
        Assert.Equal(label, DisplayHelpers.StatusLabel(code));
        Assert.Equal(css, DisplayHelpers.StatusCssClass(code));
    }

    [Fact]
    public void Status_codes_keep_their_seeded_ids()
    {
        // These numbers are the primary keys seeded into ticket_statuses; changing them breaks stored tickets.
        Assert.Equal(1, (int)TicketStatusCode.New);
        Assert.Equal(3, (int)TicketStatusCode.InProgress);
        Assert.Equal(8, (int)TicketStatusCode.Resolved);
        Assert.Equal(10, (int)TicketStatusCode.Canceled);
        Assert.Equal(10, Enum.GetValues<TicketStatusCode>().Length);
    }

    [Theory]
    [InlineData(PriorityCode.Low, "Low", "sd-priority-low")]
    [InlineData(PriorityCode.Medium, "Medium", "sd-priority-medium")]
    [InlineData(PriorityCode.High, "High", "sd-priority-high")]
    [InlineData(PriorityCode.Critical, "Critical", "sd-priority-critical")]
    public void Each_priority_has_a_label_and_css_class(PriorityCode code, string label, string css)
    {
        Assert.Equal(label, DisplayHelpers.PriorityLabel(code));
        Assert.Equal(css, DisplayHelpers.PriorityCssClass(code));
    }

    [Fact]
    public void Every_sla_state_has_a_class_and_an_icon()
    {
        foreach (var state in Enum.GetValues<SlaState>())
        {
            Assert.DoesNotContain("unknown", DisplayHelpers.SlaCssClass(state), StringComparison.Ordinal);
            Assert.NotEqual("question-circle", DisplayHelpers.SlaIcon(state));
        }
    }

    [Fact]
    public void Undefined_values_fall_back_instead_of_throwing()
    {
        Assert.Equal("Unknown", DisplayHelpers.StatusLabel((TicketStatusCode)99));
        Assert.Equal("sd-status-unknown", DisplayHelpers.StatusCssClass((TicketStatusCode)99));
        Assert.Equal("Unknown", DisplayHelpers.PriorityLabel((PriorityCode)99));
    }

    [Theory]
    [InlineData(LocalTimeFormat.Date, "Nov 20, 2026")]
    [InlineData(LocalTimeFormat.DateTime, "Nov 20, 2026 3:05 PM")]
    [InlineData(LocalTimeFormat.Time, "3:05 PM")]
    public void Local_times_use_the_house_formats(LocalTimeFormat format, string expected)
    {
        var local = new DateTime(2026, 11, 20, 15, 5, 0, DateTimeKind.Unspecified);
        Assert.Equal(expected, DisplayHelpers.FormatLocal(local, format));
    }

    [Theory]
    [InlineData("Juan Dela Cruz", "JC")]
    [InlineData("  maria   santos ", "MS")]
    [InlineData("Ana", "A")]
    [InlineData("", "?")]
    [InlineData(null, "?")]
    public void Initials_use_the_first_and_last_names(string? fullName, string expected)
    {
        Assert.Equal(expected, DisplayHelpers.Initials(fullName));
    }
}
