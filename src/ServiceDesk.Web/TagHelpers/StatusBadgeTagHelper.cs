using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Helpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// <c>&lt;status-badge code="InProgress" /&gt;</c> or <c>code="@ticket.Status"</c> renders a colored status pill.
/// Pass <see cref="Label"/> when the status was renamed in the database.
/// </summary>
[HtmlTargetElement("status-badge", TagStructure = TagStructure.WithoutEndTag)]
public sealed class StatusBadgeTagHelper : TagHelper
{
    public TicketStatusCode Code { get; set; }

    public string? Label { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.AddClass("sd-badge", HtmlEncoder.Default);
        output.AddClass(DisplayHelpers.StatusCssClass(Code), HtmlEncoder.Default);
        output.Content.SetContent(string.IsNullOrWhiteSpace(Label) ? DisplayHelpers.StatusLabel(Code) : Label);
    }
}
