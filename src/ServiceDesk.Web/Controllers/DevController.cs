using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceDesk.Application.Interfaces;
using ServiceDesk.Domain.Enums;
using ServiceDesk.Web.Filters;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels;

namespace ServiceDesk.Web.Controllers;

/// <summary>
/// Developer pages that show every shared UI component with sample data, for visual checks on
/// desktop and phone. Only reachable in the Development environment; elsewhere they answer 404.
/// Nothing here reads or writes real data.
/// </summary>
[DevelopmentOnly]
[Route("dev")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class DevController(IClock clock) : Controller
{
    [HttpGet("ui")]
    public IActionResult Ui() => View(BuildUiKit(new UiKitFormViewModel()));

    [HttpPost("ui/form")]
    public IActionResult SubmitForm(UiKitFormViewModel form)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Ui), BuildUiKit(form));
        }

        TempData.ToastSuccess($"Sample ticket \"{form.Subject}\" passed validation. Nothing was saved.");
        return RedirectToAction(nameof(Ui));
    }

    [HttpPost("ui/toast")]
    public IActionResult Toast(string kind)
    {
        switch (kind)
        {
            case "error":
                TempData.ToastError("We couldn't save the ticket. Check your connection and try again.");
                break;
            case "info":
                TempData.ToastInfo("Ticket MIS-26K011 was assigned to you.");
                break;
            case "warning":
                TempData.ToastWarning("MIS-26K013 is due in 25 minutes.");
                break;
            default:
                TempData.ToastSuccess("Ticket MIS-26K014 created.");
                break;
        }

        return RedirectToAction(nameof(Ui));
    }

    [HttpPost("ui/confirm")]
    public IActionResult Confirm()
    {
        TempData.ToastSuccess("The sample category moved to the Recycle Bin. (Preview only: nothing was deleted.)");
        return RedirectToAction(nameof(Ui));
    }

    [HttpGet("sign-in")]
    public IActionResult SignIn() => View(new SignInPreviewViewModel());

    [HttpPost("sign-in")]
    public IActionResult SignIn(SignInPreviewViewModel model)
    {
        if (ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "This page only previews the sign-in layout. It isn't connected to any accounts.");
        }

        return View(model);
    }

    private UiKitViewModel BuildUiKit(UiKitFormViewModel form)
    {
        form.Categories =
        [
            new SelectListItem("Choose a category", string.Empty),
            new SelectListItem("Network and internet", "network"),
            new SelectListItem("Computers and printers", "hardware"),
            new SelectListItem("Software and accounts", "software"),
            new SelectListItem("University information systems", "uis"),
            new SelectListItem("Other", "other"),
        ];
        form.Priorities =
        [
            new SelectListItem(DisplayHelpers.PriorityLabel(PriorityCode.Low), nameof(PriorityCode.Low)),
            new SelectListItem(DisplayHelpers.PriorityLabel(PriorityCode.Medium), nameof(PriorityCode.Medium)),
            new SelectListItem(DisplayHelpers.PriorityLabel(PriorityCode.High), nameof(PriorityCode.High)),
        ];

        var now = clock.UtcNow;
        return new UiKitViewModel
        {
            NowUtc = now,
            Form = form,
            SampleTickets =
            [
                new("MIS-26K014", "Printer in the records section won't print", "Juan Dela Cruz", "University Registrar", TicketStatusCode.New, PriorityCode.Medium, now.AddMinutes(-35), SlaState.OnTrack, "Response due in 7h 25m"),
                new("MIS-26K013", "Portal password reset for a new faculty account", "Maria Santos", "College of Education", TicketStatusCode.InProgress, PriorityCode.High, now.AddHours(-3), SlaState.AtRisk, "Due in 25 min"),
                new("MIS-26K012", "Replace the LAN cable in Room 204", "Pedro Reyes", "College of Engineering", TicketStatusCode.PendingThirdParty, PriorityCode.Low, now.AddDays(-2), SlaState.Paused, "Paused: waiting on supplier"),
                new("MIS-26K011", "Grade submission fails in the information system", "Ana Garcia", "Graduate School", TicketStatusCode.Reopened, PriorityCode.Critical, now.AddHours(-9), SlaState.Breached, "Overdue by 1h 05m"),
                new("MIS-26K010", "Install an office suite on a faculty laptop", "Jose Mendoza", "College of Management", TicketStatusCode.Resolved, PriorityCode.Low, now.AddDays(-1), SlaState.Met, "Met"),
                new("MIS-26K009", "Wi-Fi keeps dropping at the payment counter", "Rosa Aquino", "Cashier's Office", TicketStatusCode.OnHold, PriorityCode.Medium, now.AddDays(-3), SlaState.Paused, "Paused: on hold"),
                new("MIS-26K008", "New email account for a staff member", "Carlos Ramos", "Human Resources", TicketStatusCode.Closed, PriorityCode.Low, now.AddDays(-6), SlaState.Met, "Met"),
                new("MIS-26K007", "Duplicate request for projector setup", "Liza Bautista", "College of Criminology", TicketStatusCode.Canceled, PriorityCode.Low, now.AddDays(-7), SlaState.Met, "Not measured"),
            ],
        };
    }
}
