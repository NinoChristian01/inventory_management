using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace ServiceDesk.Web.TagHelpers;

/// <summary>
/// Sidebar link: <c>&lt;nav-link controller="Home" icon="grid-1x2"&gt;Dashboard&lt;/nav-link&gt;</c>.
/// Marks itself active (with aria-current) for the current page, and carries its label as a
/// tooltip for the collapsed sidebar.
/// </summary>
[HtmlTargetElement("nav-link", Attributes = "controller")]
public sealed class NavLinkTagHelper(IUrlHelperFactory urlHelperFactory) : TagHelper
{
    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = default!;

    public string Controller { get; set; } = string.Empty;

    public string Action { get; set; } = "Index";

    /// <summary>Bootstrap Icons name without the "bi-" prefix.</summary>
    public string Icon { get; set; } = "circle";

    /// <summary>When true (the default), any action of the controller marks the link active.</summary>
    public bool MatchController { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);

        var url = urlHelperFactory.GetUrlHelper(ViewContext).Action(Action, Controller) ?? "#";
        var route = ViewContext.RouteData.Values;
        var sameController = string.Equals(route["controller"] as string, Controller, StringComparison.OrdinalIgnoreCase);
        var sameAction = string.Equals(route["action"] as string, Action, StringComparison.OrdinalIgnoreCase);
        var active = sameController && (MatchController || sameAction);

        var label = (await output.GetChildContentAsync()).GetContent(HtmlEncoder.Default);

        output.TagName = "a";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("href", url);
        output.Attributes.SetAttribute("title", new HtmlString(label));
        output.AddClass("sd-nav-link", HtmlEncoder.Default);
        if (active)
        {
            output.AddClass("active", HtmlEncoder.Default);
            output.Attributes.SetAttribute("aria-current", "page");
        }

        output.Content.SetHtmlContent(
            $"<i class=\"bi bi-{HtmlEncoder.Default.Encode(Icon)} sd-nav-icon\" aria-hidden=\"true\"></i><span class=\"sd-nav-label\">{label}</span>");
    }
}
