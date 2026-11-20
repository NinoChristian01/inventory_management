using System.Net;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using ServiceDesk.Application.Common;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Infrastructure.Time;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.TagHelpers;

namespace ServiceDesk.UnitTests.Web;

public sealed class TagHelperTests
{
    [Fact]
    public void Status_badge_renders_a_pill_with_the_default_label()
    {
        var html = Render(new StatusBadgeTagHelper { Code = TicketStatusCode.PendingThirdParty }, "status-badge");

        Assert.Equal("<span class=\"sd-badge sd-status-pending-third-party\">Pending Third Party</span>", html);
    }

    [Fact]
    public void Status_badge_prefers_a_renamed_label_and_encodes_it()
    {
        var html = Render(new StatusBadgeTagHelper { Code = TicketStatusCode.OnHold, Label = "Parked <b>" }, "status-badge");

        Assert.Contains("Parked &lt;b&gt;", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<b>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Priority_indicator_always_writes_the_name()
    {
        var html = Render(new PriorityTagHelper { Code = PriorityCode.Critical }, "priority-indicator");

        Assert.Equal("<span class=\"sd-priority sd-priority-critical\">Critical</span>", html);
    }

    [Fact]
    public void Sla_chip_renders_an_icon_and_encoded_text()
    {
        var html = Render(new SlaChipTagHelper { State = SlaState.Breached, Text = "Overdue by 1h <script>" }, "sla-chip");

        Assert.StartsWith("<span class=\"sd-sla sd-sla-breached\"><i class=\"bi bi-exclamation-octagon\" aria-hidden=\"true\"></i>", html, StringComparison.Ordinal);
        Assert.Contains("Overdue by 1h &lt;script&gt;", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_time_shows_manila_time_and_keeps_utc_in_the_datetime_attribute()
    {
        var helper = new LocalTimeTagHelper(ManilaClock())
        {
            Value = new DateTime(2026, 11, 20, 2, 0, 0, DateTimeKind.Utc),
        };

        var html = Render(helper, "local-time");

        Assert.StartsWith("<time datetime=\"2026-11-20T02:00:00Z\"", html, StringComparison.Ordinal);
        Assert.Contains(">Nov 20, 2026 10:00 AM</time>", html, StringComparison.Ordinal);
        Assert.Contains("(Asia/Manila)", html, StringComparison.Ordinal);
    }

    [Fact]
    public void Local_time_treats_database_values_without_a_kind_as_utc()
    {
        var helper = new LocalTimeTagHelper(ManilaClock())
        {
            Value = new DateTime(2026, 11, 20, 16, 30, 0, DateTimeKind.Unspecified),
            Format = LocalTimeFormat.Date,
        };

        Assert.Contains(">Nov 21, 2026</time>", Render(helper, "local-time"), StringComparison.Ordinal);
    }

    [Fact]
    public void Local_time_shows_a_dash_when_empty()
    {
        var html = Render(new LocalTimeTagHelper(ManilaClock()), "local-time");

        Assert.Equal("<span class=\"text-body-secondary\">—</span>", WebUtility.HtmlDecode(html));
    }

    private static SystemClock ManilaClock() =>
        new(new FakeTimeProvider(new DateTimeOffset(2026, 11, 20, 0, 0, 0, TimeSpan.Zero)),
            Options.Create(new AppOptions { TimeZone = "Asia/Manila", OrganizationName = "MIS" }));

    private static string Render(TagHelper helper, string tagName)
    {
        var context = new TagHelperContext(tagName, [], new Dictionary<object, object>(), "test");
        var output = new TagHelperOutput(tagName, [], (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

        helper.Process(context, output);

        using var writer = new StringWriter();
        output.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
