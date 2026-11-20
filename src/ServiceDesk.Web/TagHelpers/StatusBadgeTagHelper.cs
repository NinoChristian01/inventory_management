using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// Renders a colored status pill. Accepts enum names/codes as strings (e.g. "InProgress").
/// </summary>
[HtmlTargetElement("status-badge", TagStructure = TagStructure.WithoutEndTag)]
public sealed class StatusBadgeTagHelper : TagHelper
{
    public string? Code { get; set; }
    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sd-badge", HtmlEncoder.Default);

        if (Enum.TryParse<TicketStatusCode>(Code, ignoreCase: true, out var parsed))
        {
            output.AddClass(DisplayHelpers.StatusCssClass(parsed), HtmlEncoder.Default);
            output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? DisplayHelpers.StatusLabel(parsed) : Label);
            return;
        }

        var fallback = (Code ?? string.Empty).Trim();
        output.AddClass("sd-status-unknown", HtmlEncoder.Default);
        output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? (string.IsNullOrEmpty(fallback) ? "Unknown" : fallback) : Label);
    }
}
