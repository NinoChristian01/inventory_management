using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// Renders a colored priority indicator. Accepts enum names/codes as strings (e.g. "High").
/// </summary>
[HtmlTargetElement("priority-indicator", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PriorityTagHelper : TagHelper
{
    public string? Code { get; set; }
    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sd-priority", HtmlEncoder.Default);

        if (Enum.TryParse<PriorityCode>(Code, ignoreCase: true, out var parsed))
        {
            output.AddClass(DisplayHelpers.PriorityCssClass(parsed), HtmlEncoder.Default);
            output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? DisplayHelpers.PriorityLabel(parsed) : Label);
            return;
        }

        var fallback = (Code ?? string.Empty).Trim();
        output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? (string.IsNullOrEmpty(fallback) ? "Unknown" : fallback) : Label);
    }
}
