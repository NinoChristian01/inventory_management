using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Application.Tickets;

namespace ServiceDesk.Web.ViewModels.Tickets;

public sealed class TicketIndexViewModel
{
    public string? Search { get; set; }
    public short? StatusId { get; set; }
    public short? PriorityId { get; set; }
    public int? TeamId { get; set; }
    public bool OpenOnly { get; set; } = true;
    public string Sort { get; set; } = "updated_desc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public IReadOnlyList<TicketListItemDto> Items { get; set; } = [];
    public IReadOnlyList<SelectListItem> Statuses { get; set; } = [];
    public IReadOnlyList<SelectListItem> Priorities { get; set; } = [];
    public IReadOnlyList<SelectListItem> Teams { get; set; } = [];
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize)));
}
