using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ServiceDesk.Web.ViewModels.Tickets;

public sealed class CreateTicketViewModel
{
    [Required, StringLength(255)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(8000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Priority")]
    public short PriorityId { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Display(Name = "Team")]
    public int? AssignedTeamId { get; set; }

    public IReadOnlyList<SelectListItem> Priorities { get; set; } = [];
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];
    public IReadOnlyList<SelectListItem> Teams { get; set; } = [];
}
