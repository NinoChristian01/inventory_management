# Phase 2: Database and entities

Status: **in progress / artifacts implemented** (working tree, 20 November 2026)

## Scope (planned)

* 27 entities, EF Core configurations, `InitialCreate` migration + idempotent SQL export, seeder/reference data, and legacy `rqst` import.
* **Done when:** an empty database builds from migrations; import gives 65 teams, 788 users, and 1,124 tickets.

## Implemented in the working tree

* **Domain entities** (`src/ServiceDesk.Domain/Entities/`): Identity types (`User`, `Role`, `UserClaim`, `RoleClaim`, `UserLogin`, `UserRole`, `UserToken`), org/team (`Team`, `TeamMember`), ticket core and related (`Ticket`, `TicketAssignee`, `TicketWatcher`, `TicketComment`, `TicketAttachment`, `TicketActivityLog`, `TicketSla`), lookups/config (`TicketStatus`, `TicketPriority`, `TicketCategory`, `SlaPolicy`, `BusinessHour`, `Holiday`), and system records (`Notification`, `AuditLog`, `DeletedRecord`, `AppSetting`, `TicketNumberSequence`).
* **Persistence** (`src/ServiceDesk.Infrastructure/Persistence/`): `AppDbContext` (Identity DbContext with snake_case naming convention) and entity configurations for Identity joins, users/roles, teams, ticket core/related entities, and system/config tables.
* **EF Core migration**: `20261120043819_InitialCreate` with model snapshot.
* **SQL exports/scripts** (`database/`):
  * `001_initial_create.sql` — idempotent migration script.
  * `002_seed_and_import_rqst.sql` — seeds reference data (statuses, priorities, system roles, default category, business hours, SLA policies, app settings) and imports legacy data from `rqst.*` (teams/offices, users/employees, tickets/requests, assignees, roles derived from personnel/leads, ticket number sequence). Includes timezone normalization (UTC+8 → UTC), clamping/cleanup for legacy timestamp issues, requester/employee-number handling, and validation summary queries.
* **Infrastructure wiring**: MySQL/MariaDB DbContext registration via `Microting.EntityFrameworkCore.MySql` with `ServerVersion.AutoDetect` fallback; connection strings configured in appsettings.

## Verification status

* `dotnet build ServiceDesk.slnx` succeeds (0 warnings, 0 errors).
* Existing test suite passes (71 tests) on the current baseline.
* **Database/import end-to-end validation** (empty DB from migrations; legacy import counts 65/788/1124) requires a reachable MariaDB/MySQL server and the legacy `rqst` source data loaded; validate in the target environment by applying `001_initial_create.sql` (or `dotnet ef database update`) and running `002_seed_and_import_rqst.sql`, then checking the script's summary output.

## Notes / follow-ups

* Ensure `rqst.sql` remains out of source control (contains personal data).
* Confirm least-privilege database users/grants per deployment guidance before production use.
* Add a formal phase sign-off after environment validation of migration + import counts.
