# ServiceDesk

IT service management and help desk system for the Management Information System office. It
replaces the legacy `rqst` request log with tracked tickets, assignment, conversation, SLA
monitoring, notifications and reports.

ASP.NET Core MVC on .NET 10 LTS, EF Core 10 with MariaDB or MySQL, ASP.NET Core Identity,
Bootstrap 5, SignalR.

## Status

| Phase | Scope | Status |
|---|---|---|
| 1 | Foundation: solution, pipeline, security headers, UI shell | Done |
| 2 | Database and entities, legacy import | Next |
| 3 | Sign-in and access control | Planned |
| 4 | Users, teams and reference data | Planned |
| 5 | Tickets | Planned |
| 6 | Assignment, conversation and history | Planned |
| 7 | SLA and notifications | Planned |
| 8 | Dashboards and reports | Planned |
| 9 | Security testing and performance | Planned |
| 10 | Deployment and documentation | Planned |

## Getting started

Requirements: .NET SDK 10.0.202 or later (10.0 band). From Phase 2: MariaDB 10.4+ or MySQL 8.0+.

```powershell
dotnet tool restore
dotnet build ServiceDesk.slnx
dotnet test --solution ServiceDesk.slnx
dotnet run --project src/ServiceDesk.Web --launch-profile https   # https://localhost:7080
```

In Development, the sidebar also links to the UI kit (`/dev/ui`) and a sign-in preview (`/dev/sign-in`).

## Layout

| Path | What it holds |
|---|---|
| `src/ServiceDesk.Domain` | Entities, enums and business rules. References nothing else. |
| `src/ServiceDesk.Application` | Use cases, DTOs, validators, and the interfaces Infrastructure implements. |
| `src/ServiceDesk.Infrastructure` | EF Core and the MySQL provider, Identity stores, files, background jobs. |
| `src/ServiceDesk.Web` | MVC, API, SignalR hub, middleware; wires everything at startup. |
| `tests/` | Unit tests and in-memory integration tests. |
| `docs/` | [Architecture](docs/architecture.md) and one report per phase in [docs/phases](docs/phases). |
| `ticketing-system/` | The original design notes, schema draft and legacy migration draft (reference only). |

`rqst.sql`, the legacy dump, stays out of Git because it holds employees' personal data.
