using System.Globalization;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// <c>&lt;local-time value="@ticket.CreatedAt" /&gt;</c> shows a stored UTC time in the
/// organization's time zone, inside a &lt;time&gt; element whose datetime attribute keeps the UTC value.
/// An empty value shows a dash.
/// </summary>
[HtmlTargetElement("local-time", TagStructure = TagStructure.WithoutEndTag)]
public sealed class LocalTimeTagHelper(IClock clock) : TagHelper
{
    public DateTime? Value { get; set; }

    public LocalTimeFormat Format { get; set; } = LocalTimeFormat.DateTime;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagMode = TagMode.StartTagAndEndTag;

        if (Value is not { } value)
        {
            output.TagName = "span";
            output.AddClass("text-body-secondary", HtmlEncoder.Default);
            output.Content.SetContent("—");
            return;
        }

        var local = clock.ToLocal(value);
        var utc = clock.ToUtc(local);
        output.TagName = "time";
        output.Attributes.SetAttribute("datetime", utc.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
        output.Attributes.SetAttribute("title", $"{DisplayHelpers.FormatLocal(local, LocalTimeFormat.DateTime)} ({clock.TimeZone.Id})");
        output.Content.SetContent(DisplayHelpers.FormatLocal(local, Format));
    }
}
