# ServiceDesk Project Summary

## 1. Project Overview

**Project:** ServiceDesk / Ticketing System  
**Project directory:** `C:\Users\User\Desktop\opencode_environment`  
**Purpose:** Replace or modernize the existing request/ticketing system using ASP.NET Core MVC, C#, and MariaDB/MySQL while preserving historical request data and improving security, maintainability, and usability.

The project is being developed in phases. The design review was completed first, and **Phase 1 (Foundation) has been implemented, tested, and committed**. Phase 2 (Database and legacy-data migration) is the next step and is not yet complete in the recorded session.

## 2. Recommended Technology Stack

| Area | Selected technology |
|---|---|
| Backend | C# / ASP.NET Core MVC |
| Framework | .NET 10 |
| ORM / data access | Entity Framework Core 10 |
| Database | MariaDB for local development; supported MariaDB or MySQL for production |
| EF Core database provider | `Microting.EntityFrameworkCore.MySql` 10.0.14 (maintained Pomelo fork) |
| Authentication | ASP.NET Core Identity (planned for Phase 3) |
| Frontend framework | Bootstrap 5.3.9 |
| Icons | Bootstrap Icons 1.13.2 |
| Tables | DataTables 3.1.5 with Bootstrap 5 integration |
| Charts | Chart.js 4.6.1 |
| Client-side validation | jQuery Validation 1.22.1 and jQuery Validation Unobtrusive 4.0.0 |
| Realtime client library | Microsoft SignalR client 10.0.11; ASP.NET Core SignalR server is part of the shared framework |
| Fonts | Plus Jakarta Sans and IBM Plex Mono |
| Logging | Serilog.AspNetCore 10.0.0 and Serilog file sink 7.0.0 |
| Testing | xUnit v3 / Microsoft Testing Platform, plus ASP.NET Core integration testing |
| Frontend library management | LibMan 3.0.114 |
| EF command-line tools | `dotnet-ef` 10.0.15 |

The provider choice was made because the standard Pomelo provider had no EF Core 10 release in the package investigation. The Microting fork is based on Pomelo and its package README explicitly lists MariaDB support. The selected version targets .NET 10 and depends on EF Core Relational 10.0.14 and MySqlConnector 2.6.2.

## 3. Development Environment

The machine inspection found:

- .NET SDK: `10.0.202`
- Installed .NET runtimes included ASP.NET Core 10.0.6 and several earlier versions.
- XAMPP installed with MariaDB `10.4.32`.
- Git `2.53.0`.
- Node.js `v26.13.1`.
- MySQL Workbench 6.3 CE.
- Docker was not installed.
- The MariaDB server was initially stopped, then successfully started during the session.

**Important deployment note:** MariaDB 10.4 is outdated and no longer receives security fixes. Use a supported production database version, such as MariaDB 10.11/11.4 or MySQL 8.4. The session also recommended installing the latest .NET 10 SDK/runtime patch before deployment; the machine had runtime 10.0.6 while the current patch identified in the session was 10.0.15.

## 4. Solution Structure

The solution uses a layered architecture:

```text
opencode_environment/
├── src/
│   ├── ServiceDesk.Domain/
│   ├── ServiceDesk.Application/
│   ├── ServiceDesk.Infrastructure/
│   └── ServiceDesk.Web/
├── tests/
│   ├── ServiceDesk.UnitTests/
│   └── ServiceDesk.IntegrationTests/
├── ticketing-system/
│   ├── README.md
│   ├── schema.sql
│   └── migrate_from_rqst.sql
├── docs/
│   ├── architecture.md
│   └── phases/
│       └── 01-foundation.md
├── ServiceDesk.slnx
├── global.json
├── Directory.Build.props
├── Directory.Packages.props
├── dotnet-tools.json
└── README.md
```

### Layer responsibilities

- **Domain:** Core business vocabulary and rules, including ticket status, priority, and SLA state.
- **Application:** Use-case abstractions, application options, and business-level exceptions.
- **Infrastructure:** Database and external-service implementations, dependency injection, and system clock.
- **Web:** ASP.NET Core MVC application, request pipeline, views, UI, middleware, and web-specific services.
- **Unit tests:** Tests for individual components and rules.
- **Integration tests:** Tests that run the web application and verify its HTTP behavior.

Project references are intended to point inward, keeping the Domain layer independent of outer layers.

## 5. Legacy Database and Migration Source

The legacy database is named `rqst`. The SQL dump is `rqst.sql`, and the existing migration-related files are under `ticketing-system/`.

The legacy schema contains four tables:

- `employees`
- `offices`
- `requests`
- `vw_employees`

The data volume found in both the dump and live database:

| Table | Rows |
|---|---:|
| Offices | 65 |
| Employees | 788 |
| Requests | 1,124 |
| `vw_employees` | 608 in the dump |

The live database contains 65 offices, 788 employees, and 1,124 requests. The live `rqst` database matches the SQL dump on the checked row counts. Its latest request control number is `MIS-26K012`, and request dates range from June 18, 2024 to November 12, 2026.

### Request status counts

| Status | Count |
|---|---:|
| Rated | 973 |
| VOID / void | 139 |
| Created | 6 |
| Completed | 4 |
| Assigned | 2 |
| **Total** | **1,124** |

The existing `rqst` database and the separate `nwssu` database were explicitly treated as databases that must not be overwritten or modified during setup.

## 6. Migration Risks and Data-Quality Findings

The design review identified migration issues that must be addressed before importing the legacy records:

1. **Incorrect column mapping in the existing migration script:** the noted-by and approved-by IDs and names were in the wrong order. This could store names in numeric columns and user IDs as zero without an obvious import failure.
2. **Placeholder employee numbers:** 108 employee rows use employee number `0`. Treating this value as unique would merge multiple people into one account. Up to 657 request references involve employees whose employee number is duplicated.
3. **Missing or incomplete employee names:** the live-data profile found 3 employees without first names and 2 without last names. There are also blank or placeholder middle names.
4. **Duplicate office code:** two office records use the code `ETEEAP`.
5. **Missing request descriptions:** 72 requests have a blank `nature` field.
6. **Void records without archive dates:** all 139 void requests were reported without `date_archived`.
7. **Date inconsistencies:** many `date_received` and `date_completed` values contain midnight-only timestamps or precede the request filing timestamp. Legacy timestamps are local UTC+8 values and need deliberate timezone handling.
8. **Other data checks:** the profile found one requester ID that does not match an employee. Personnel references were otherwise matched in the parser check. Ratings were within the 1–5 range.

**Migration principle:** preserve the original data, do not silently merge people, and explicitly handle invalid or incomplete values. Keep the existing `rqst` database read-only during migration.

## 7. Approved Design Decisions

The user approved the five recommendations:

1. **Database provider:** use the maintained Microting EF Core MySQL provider fork. Develop locally against XAMPP MariaDB, but deploy to a supported MariaDB or MySQL release.
2. **Schema management:** use EF Core migrations as the schema source, and export each migration as a re-runnable SQL script.
3. **Ticket numbering:** retain the legacy ticket number format. New tickets continue after `MIS-26K012`.
4. **First login for imported employees:** an administrator activates employee accounts and issues temporary passwords. Do not use employee number plus birthday as the initial credential method.
5. **SLA calculation:** Low, Medium, and High priority tickets use business hours; Critical priority tickets use a 24/7 clock.

Other design goals include an audit log, a Recycle Bin, responsive mobile layouts, and role-based access.

## 8. Phase 1 — Foundation: Completed

Phase 1 was completed and committed to Git.

**Commit:** `8489a25`  
**Branch:** `main`  
**Commit contents:** 111 files changed, 6,538 insertions.

### Implemented

- Layered solution and two test projects.
- .NET SDK version pinning and centralized package version management.
- Build settings that treat warnings and known-vulnerable packages as build failures.
- Rolling file logging, including one request log line per request.
- Security headers on responses.
- Friendly error pages and JSON error responses for `/api`.
- Antiforgery validation for form posts.
- `/health` endpoint.
- Asia/Manila timezone display support.
- Bootstrap 5 UI shell with teal styling.
- Self-hosted fonts and a collapsible sidebar.
- Toast notifications after redirects.
- Shared confirmation dialog.
- Sign-in layout shell.
- Mobile navigation drawer, table-to-card presentation, and bottom-sheet confirmation UI.
- Shared UI components:
  - `<status-badge>`
  - `<priority-indicator>`
  - `<sla-chip>`
  - `<local-time>`
  - `<nav-link>`
- Development-only UI kit page at `/dev/ui`.

### Verification results

- Build completed with **0 warnings**.
- **71 tests passed:** 60 unit tests and 11 integration tests.
- Desktop and narrow viewport checks were performed at 390 px and 360 px using headless Edge.
- No horizontal page overflow was detected in those checks.
- Fonts loaded, and browser console checks showed no errors or Content Security Policy violations.
- The verification pass caught and fixed DataTables load order, mobile card text clipping, and missing request log entries.

### Phase 1 scope adjustments

- Sign-in is not yet enforced by default; full authentication is deferred to Phase 3 when ASP.NET Core Identity is introduced.
- The current dashboard is a system-status dashboard. Role-specific dashboards are planned for Phase 8.
- A real-device mobile check is still recommended; headless browser screenshots are not a substitute for testing on an actual phone.

## 9. Current Database Setup State and Next Phase

At the end of the recorded session:

- XAMPP MariaDB was successfully started.
- MariaDB version: `10.4.32-MariaDB`.
- The existing databases included `rqst` and `nwssu`; both were left untouched.
- The user approved using XAMPP's default local `root` account for initial database setup.
- The next planned action is Phase 2: create the new application database `ticketing_system`, a separate `ticketing_system_test` database, and a limited-permission database user for the application.
- The next phase also includes importing and transforming the legacy data safely.

**Phase 2 had not yet been completed** in the transcript. The provider API was being inspected when the session ended.

## 10. Useful Commands

Run tests:

```powershell
dotnet test --solution ServiceDesk.slnx
```

Run the web application using the HTTPS launch profile:

```powershell
dotnet run --project src/ServiceDesk.Web --launch-profile https
```

The Phase 1 report recorded the HTTPS address as `https://localhost:7080`. Check the current `launchSettings.json` before relying on a port, because an earlier scaffolded launch profile showed a different port.

## 11. Security and Repository Notes

- The legacy SQL dump contains employee names and birthdays. `rqst.sql` is explicitly ignored by Git and must never be committed.
- Runtime logs and uploaded application data are ignored.
- Local configuration overrides and secrets are ignored.
- Do not commit credentials or production connection strings.
- Use a limited-rights database account for the application rather than running the application as database root.
- Keep the legacy `rqst` database read-only throughout migration.
- Keep the existing `nwssu` database untouched.

## 12. Immediate Next Steps

1. Confirm the installed .NET 10 SDK/runtime is patched to the current supported release.
2. Complete Phase 2 database creation using the approved local MariaDB setup.
3. Create and test the limited-permission application database account.
4. Implement the EF Core 10 provider configuration and database schema migrations.
5. Correct the legacy migration column mapping.
6. Design a safe strategy for the 108 placeholder employee numbers and preserve all request-to-employee relationships.
7. Normalize or explicitly preserve inconsistent legacy timestamps; convert local UTC+8 values correctly.
8. Import into the new database and validate row counts, relationships, statuses, and ticket-number continuity.
9. Continue with authentication and account activation in Phase 3.

---

*This summary is based on the supplied project-session transcript. It records the state at the end of that transcript and does not imply that Phase 2 or later phases have already been implemented.*
