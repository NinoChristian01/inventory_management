# Admin Guide

Admins configure and oversee ServiceDesk access and operations, support teams, and handle escalated management tasks.

## Getting started
1. **Sign in** at `/account/login`.
2. If this is a fresh environment and no admin assignment existed at startup, the system may bootstrap a default admin account (when database seeding is enabled/reachable):
   - **Username:** `admin`
   - **Password:** `Admin@12345`
   - Change this password immediately after first sign-in.
3. Complete any forced password-change prompts before continuing.

## Admin capabilities (current implementation)

### User management
- Go to **Users** (`/users`) to search and review users (including imported accounts).
- **Activate users:** set a temporary password and activate an account (user will be required to change password on next sign-in).
- **Deactivate users:** disable access for accounts that should no longer sign in.
- **Manage roles:** open a user (`/users/{id}`) and assign/remove system roles (Requester, Agent, TeamLead, Admin). Role changes may require the user to sign in again for updated claims/session context.

### Team visibility
- Go to **Teams** (`/teams`) to view teams/departments, service-team indicators, status, and member counts.

### Ticket operations (full operational access)
- Access and manage tickets across the system (subject to implemented rules and UI capabilities).
- Create tickets, view details/history, post replies/internal notes, upload/download attachments, assign tickets/teams, update statuses/workflows, and watch tickets.
- Monitor workload and key metrics from the **Dashboard**.

### Notifications
- Use **Notifications** (`/notifications`) for in-app updates and mark items read/read-all.

## Recommended admin workflows

### Onboarding a user
1. Locate the user in **Users**.
2. Activate the account with a temporary password (meeting password policy).
3. Assign appropriate role(s).
4. Confirm team/department alignment (and update related configuration when management UI is available).

### Supporting operations
- Review unassigned/overdue tickets and coordinate with Team Leads.
- Ensure agents/leads have correct roles and access.
- Use ticket activity history and comments/notes for auditability and handoffs.

## Security and access notes
- The app uses authenticated access by default; API endpoints under `/api` return 401/403 for unauthorized/forbidden API requests.
- Privileged actions are permission/role controlled (e.g. user administration, workflow actions, internal notes visibility).
- Enforce strong password practices and rotate/bootstrap credentials in non-development environments.

## Current limitations / areas that may still be in progress
Depending on the current build/environment, some admin features may be partially implemented or pending:
- Full CRUD administration for categories/priorities/statuses, SLA policies, business hours/holidays, and team membership
- Audit log UI and recycle bin/restore management
- Automated SLA breach/warning jobs, auto-close/escalations, and email/real-time integrations beyond in-app notifications
- Reporting dashboards/exports (Excel/PDF/CSV) and expanded API coverage
- Additional hardening, deployment automation, and operational runbooks

Refer to project docs (e.g. `docs/architecture.md`, `docs/phases/*.md`) and the current UI for the authoritative state of available functionality.