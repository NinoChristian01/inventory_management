using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// <c>&lt;priority-indicator code="High" /&gt;</c> or <c>code="@ticket.Priority"</c> renders a colored dot and the priority
/// name. The name is always written out, so the meaning never depends on color alone.
/// </summary>
[HtmlTargetElement("priority-indicator", TagStructure = TagStructure.WithoutEndTag)]
public sealed class PriorityTagHelper : TagHelper
{
    public PriorityCode Code { get; set; }

    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sd-priority", HtmlEncoder.Default);
        output.AddClass(DisplayHelpers.PriorityCssClass(Code), HtmlEncoder.Default);
        output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? DisplayHelpers.PriorityLabel(Code) : Label);
    }
}
