# Phase 1: Foundation

Status: **done** (20 November 2026). Version 0.1.0.

## What this phase delivers

The solution skeleton, the build rules every later phase relies on, the hardened request pipeline,
and the UI shell that every feature page will sit in. There is no database, sign-in or ticket data
yet; those arrive in Phases 2–5.

* **Four projects plus two test projects**, with references pointing inward
  (Web → Application → Domain, Infrastructure → Application).
* **Build rules:** .NET SDK pinned in `global.json`; nullable reference types; warnings fail the
  build; analyzers and code-style rules enforced at build time; every NuGet version in one file
  (`Directory.Packages.props`); vulnerable packages fail the build.
* **Pipeline** (`src/ServiceDesk.Web/Program.cs`, in banner-commented sections):
  * Serilog: console, `logs/servicedesk-*.log` and `logs/errors-*.log` under the content root, one
    line per request (health probes and static files kept out of the normal log), plus
    `logs/fatal-*.log` for startup and unobserved failures.
  * Security headers on every response, error pages included (see `Middleware/`).
  * Friendly error pages for pages; problem details (JSON) for `/api`; internal error text never
    reaches users.
  * Antiforgery validation on every POST, PUT, PATCH and DELETE.
  * `/health` (JSON) and the `en-PH` culture with the Asia/Manila time zone.
* **UI shell:** Bootstrap 5.3 themed to the ServiceDesk palette, self-hosted Plus Jakarta Sans and
  IBM Plex Mono, collapsible sidebar (remembered per browser), top bar with breadcrumb and page
  title, toasts after redirects, one shared confirm dialog, sign-in layout, phone layer
  (drawer menu, 44 px targets, tables as cards, bottom-sheet confirm, toasts under the top bar).
* **Shared components** as tag helpers: `<status-badge>`, `<priority-indicator>`, `<sla-chip>`,
  `<local-time>`, `<nav-link>`.
* **Developer UI kit** at `/dev/ui` and a sign-in preview at `/dev/sign-in`, shown only in
  Development (404 elsewhere), with sample data for visual checks.

## Changes from the plan

* **"Sign-in required by default" moves to Phase 3.** It needs ASP.NET Core Identity, which Phase 3
  adds. Everything else in the plan's Phase 1 is here.
* **The dashboard shows system status for now** (health, version, environment, time zone). Role
  dashboards arrive in Phase 8.
* **The tool manifest is `dotnet-tools.json` in the repository root,** where the .NET 10 SDK now
  creates it, instead of `.config/`.
* **DataTables 3 needs no jQuery,** so jQuery loads only on pages with forms, for client-side validation.
* **The phone breakpoint is Bootstrap's `lg` (992 px)** instead of EPES's 860 px, so the drawer menu
  and the phone stylesheet switch at the same width.
* **Non-ASCII text is written as-is** (for example ñ in names) instead of as `&#x..;` entities.
  HTML-sensitive characters are still escaped.
* Chart.js and the SignalR client are added to `libman.json` in Phases 7–8, when they're used.

## Files

```text
global.json                      SDK 10.0.202 (rolls forward to newer 10.0 feature bands); test runner
Directory.Build.props            shared build rules (net10.0, nullable, warnings as errors, audit)
Directory.Packages.props         every NuGet version
dotnet-tools.json                dotnet-ef 10.0.15, libman 3.0.114
.editorconfig  .gitattributes  .gitignore   (rqst.sql, logs/ and App_Data/ are never committed)
ServiceDesk.slnx
docs/architecture.md             the approved design
src/ServiceDesk.Domain/Enums/                TicketStatusCode (fixed ids 1–10), PriorityCode, SlaState
src/ServiceDesk.Application/
  Common/AppOptions.cs                       "App" settings: name, office, time zone
  Interfaces/IClock.cs                       the one place the app reads the time
  Exceptions/                                BusinessRule, NotFound, ForbiddenAccess, ConcurrencyConflict
src/ServiceDesk.Infrastructure/
  Time/SystemClock.cs, AppOptionsValidator.cs
  DependenciesInjection.cs                   AddInfrastructure()
src/ServiceDesk.Web/
  Program.cs
  Logging/        LoggingSetup, LogFileOptions, RequestLogLevel
  ErrorHandling/  ApiRequest, ExceptionStatusMapper, GlobalExceptionHandler, StatusCodeText
  Middleware/     ContentSecurityPolicy, SecurityHeadersMiddleware
  Health/         HealthResponseWriter
  Filters/        DevelopmentOnlyAttribute
  Helpers/        DisplayHelpers, AppVersion, Toasts
  TagHelpers/     StatusBadge, Priority, SlaChip, LocalTime, NavLink
  Controllers/    HomeController, ErrorController, DevController
  ViewModels/     ErrorViewModel, SystemStatusViewModel, UiKitViewModels
  Views/          _Layout, _AuthLayout, _Sidebar, _Toasts, _ConfirmModal, _ValidationScriptsPartial,
                  Error, Home/Index, Dev/Ui, Dev/SignIn
  wwwroot/        css/site.css, css/site.mobile.css, js/site.js, js/sidebar-state.js, js/dev-ui.js,
                  img/favicon.svg, lib/ (restored by LibMan and committed, so builds work offline)
  libman.json  appsettings.json  appsettings.Development.json  Properties/launchSettings.json
tests/ServiceDesk.UnitTests/         display helpers, tag helpers, clock, options, errors, CSP, log levels
tests/ServiceDesk.IntegrationTests/  the app in memory: shell, headers, errors, health, antiforgery, toasts
```

## Packages

| Package | Version | Project |
|---|---|---|
| Serilog.AspNetCore | 10.0.0 | Web |
| xunit.v3 | 4.0.3 | both test projects |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.15 | IntegrationTests |
| Microsoft.Extensions.TimeProvider.Testing | 10.11.1 | UnitTests |

Client libraries (LibMan, `src/ServiceDesk.Web/libman.json`): Bootstrap 5.3.9, Bootstrap Icons
1.13.2, DataTables 3.1.5 with Bootstrap 5 styling, jQuery 3.7.1, jquery-validation 1.21.0,
jquery-validation-unobtrusive 4.0.0, Plus Jakarta Sans and IBM Plex Mono (Fontsource 5.3.0, Latin subset).

## Database

None in this phase. Phase 2 adds the DbContext, the `InitialCreate` migration and its SQL export.

## How to run

```powershell
dotnet tool restore
dotnet build ServiceDesk.slnx
dotnet run --project src/ServiceDesk.Web --launch-profile https   # https://localhost:7080
```

Use `--launch-profile http` for <http://localhost:5080> if the development certificate isn't
trusted yet (`dotnet dev-certs https --trust` trusts it). Client libraries are already in
`wwwroot/lib`; after changing `libman.json`, run `dotnet tool run libman restore` in `src/ServiceDesk.Web`.

## How to test

```powershell
dotnet test --solution ServiceDesk.slnx
```

71 tests: 60 unit and 11 integration. The integration tests start the real app in memory, as
Production and as Development. They check the shell, the security headers, the friendly 404 page,
problem details for `/api`, `/health`, that developer pages are hidden in Production, that a POST
without an antiforgery token is refused, that a toast survives the redirect after a POST and shows
only once, and that server-side validation messages appear.

Manual checks, done for this phase with headless Edge at 1440×900, 390×844 and 360×740:

1. `/`: dashboard with System status **Healthy** and local time in Asia/Manila.
2. Sidebar button (desktop): collapses to icons and stays collapsed after a reload.
3. `/dev/ui`: all ten status badges, priorities, SLA chips; the sample table pages 5 rows at a time
   and searches; on a phone each row is a labeled card with the subject on top.
4. **Delete category** on `/dev/ui`: the confirm dialog opens (a bottom sheet on phones, main button
   on top); **Move to Recycle Bin** shows a success toast.
5. The toast buttons: success, info and warning toasts fade after 6 seconds; error toasts stay
   until closed.
6. **Submit sample** with empty fields: messages appear without a page load (client-side);
   with JavaScript off, the server returns the same messages.
7. `/no-such-page`: the friendly 404 page.
8. No page scrolls sideways at 390 px or 360 px.

Result for this phase: no sideways scroll on any page, both fonts load, and the browser console
shows no errors or Content-Security-Policy violations.

## Building on this phase

* Every page sets `ViewData["Title"]`, and `ViewData["Parent"]` / `ViewData["ParentUrl"]` for the
  breadcrumb.
* No inline `<script>`: the Content-Security-Policy blocks it. Put JavaScript in `wwwroot/js` and pass
  data through `data-*` attributes or `<script type="application/json">` blocks.
* After a successful POST, redirect and call `TempData.ToastSuccess("…")` (or `ToastError`,
  `ToastInfo`, `ToastWarning`).
* Destructive buttons get `data-confirm-title`, `data-confirm-text` (saying what will happen),
  `data-confirm-ok` and `data-confirm-cancel`.
* Tables that should become cards on phones get the `sd-stack` class; mark the title cell `m-title`
  and hide cells with `m-hide`. Call `sd.labelTables(container)` after inserting rows.
* Enum tag-helper attributes take a member name (`code="InProgress"`) or an expression
  (`code="@ticket.Status"`).
* Use `IClock` for the time and `<local-time value="…">` to display stored UTC times.

## Follow-ups

* This machine has .NET runtime 10.0.6; the current patch is 10.0.15. Install the latest .NET 10 SDK
  before deployment to pick up security fixes.
