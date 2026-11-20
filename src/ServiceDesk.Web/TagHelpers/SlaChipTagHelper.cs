using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// <c>&lt;sla-chip state="AtRisk" text="Due in 25 min" /&gt;</c> renders an icon and text
/// colored by how the SLA clock stands.
/// </summary>
[HtmlTargetElement("sla-chip", TagStructure = TagStructure.WithoutEndTag)]
public sealed class SlaChipTagHelper : TagHelper
{
    public SlaState State { get; set; }

    public string Text { get; set; } = string.Empty;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sd-sla", HtmlEncoder.Default);
        output.AddClass(DisplayHelpers.SlaCssClass(State), HtmlEncoder.Default);
        output.Content.AppendHtml($"<i class=\"bi bi-{DisplayHelpers.SlaIcon(State)}\" aria-hidden=\"true\"></i>");
        output.Content.Append(Text);
    }
}
