using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Domain.Enums;

namespace ServiceDesk.Web.ViewModels;

/// <summary>The developer UI kit page: every shared component, filled with sample data.</summary>
public sealed class UiKitViewModel
{
    public required IReadOnlyList<SampleTicketRow> SampleTickets { get; init; }

    public required UiKitFormViewModel Form { get; init; }

    public DateTime NowUtc { get; init; }
}

/// <summary>A made-up ticket row for the UI kit's sample table.</summary>
public sealed record SampleTicketRow(
    string Number,
    string Subject,
    string Requester,
    string Department,
    TicketStatusCode Status,
    PriorityCode Priority,
    DateTime CreatedUtc,
    SlaState Sla,
    string SlaText);

/// <summary>Sample form showing field styles and both client-side and server-side validation.</summary>
public sealed class UiKitFormViewModel
{
    [Required(ErrorMessage = "Enter a subject.")]
    [StringLength(255, ErrorMessage = "Keep the subject under 255 characters.")]
    [Display(Name = "Subject")]
    public string? Subject { get; set; }

    [Required(ErrorMessage = "Choose a category.")]
    [Display(Name = "Category")]
    public string? Category { get; set; }

    [Display(Name = "Priority")]
    public PriorityCode Priority { get; set; } = PriorityCode.Medium;

    [Required(ErrorMessage = "Describe the problem.")]
    [StringLength(4000, MinimumLength = 10, ErrorMessage = "Describe the problem in at least 10 characters.")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = [];

    public IEnumerable<SelectListItem> Priorities { get; set; } = [];
}

/// <summary>Fields of the sign-in layout preview.</summary>
public sealed class SignInPreviewViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    [Display(Name = "Username")]
    public string? UserName { get; set; }

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Display(Name = "Keep me signed in on this computer")]
    public bool RememberMe { get; set; }
}
