using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Application.Tickets;

namespace ServiceDesk.Web.ViewModels.Tickets;

public sealed class TicketDetailsViewModel
{
    public TicketDetailsDto Details { get; set; } = default!;
    public bool CanViewInternalNotes { get; set; }
    public bool CanAssign { get; set; }
    public bool CanChangeStatus { get; set; }
    public bool IsWatching { get; set; }

    public AddCommentInput Comment { get; set; } = new();
    public ChangeStatusInput ChangeStatus { get; set; } = new();
    public AssignInput Assign { get; set; } = new();

    public IReadOnlyList<SelectListItem> Statuses { get; set; } = [];
    public IReadOnlyList<SelectListItem> Teams { get; set; } = [];
    public IReadOnlyList<SelectListItem> Users { get; set; } = [];
}

public sealed class AddCommentInput
{
    [Required, StringLength(4000)]
    public string Body { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}

public sealed class ChangeStatusInput
{
    [Required]
    public short NewStatusId { get; set; }
    [StringLength(2000)]
    public string? Note { get; set; }
    [StringLength(2000)]
    public string? ResolutionSummary { get; set; }
}

public sealed class AssignInput
{
    public int? TeamId { get; set; }
    public long? AssigneeUserId { get; set; }
    public bool MakePrimary { get; set; } = true;
}
