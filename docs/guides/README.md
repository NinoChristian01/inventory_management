# ServiceDesk Role Guides

This folder contains role-based usage guides for the ServiceDesk web application.

## Roles
- **[Requester](./requester.md)** – submits and tracks their own requests/tickets.
- **[Agent](./agent.md)** – handles tickets for their team, collaborates, and updates progress.
- **[Team Lead](./team-lead.md)** – oversees team ticket handling, assignment, and user visibility for their area.
- **[Admin](./admin.md)** – manages system access, roles, and overall helpdesk operations.

## Common access
- **URL:** run the MVC app (`dotnet run --project src/ServiceDesk.Web --launch-profile https`) and open the site (e.g. `https://localhost:<port>`).
- **Sign in:** `/account/login` (username + password).
- **First sign-in / activated accounts:** users may be required to change their password before using the system.
- **Notifications:** `/notifications` (unread badge shown in the sidebar when signed in).
- **Dashboard:** `/` shows ticket metrics (scoped to your access) and recent tickets.

## Notes
- Permissions and visible tickets depend on your assigned role(s) and access scope.
- Some administrative/reference-data and automation features (full CRUD for all catalogs/SLA/business hours, reports/exports, background SLA jobs, email delivery, etc.) may still be limited or in progress depending on the current deployment/build.
- API endpoints are available under `/api/v1/tickets` for authenticated/integrated use (subject to permissions and access scope).