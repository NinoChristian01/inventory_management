# Agent Guide

Agents handle assigned/team tickets, communicate with requesters, update ticket progress, and collaborate using internal notes and notifications.

## Getting started
1. **Sign in** at `/account/login`.
2. Change your password if prompted (e.g. first sign-in or after account activation/reset).
3. Use the **Dashboard** (`/`) to monitor open/overdue/related tickets and recent updates within your access scope.

## Core responsibilities and actions

### Work the ticket queue
- Go to **Tickets** (`/tickets`) to search and filter tickets.
- Open tickets to review request details, history, attachments, SLA due dates (when available), and activity.
- Ticket visibility is scoped by permissions/access rules (typically team/assigned/related access for agents).

### Create tickets (if needed)
- Use **New ticket** (`/tickets/create`) to log requests on behalf of users or internal work, subject to permissions.

### Communicate and document work
- **Public replies:** respond to the requester in the ticket conversation.
- **Internal notes:** add internal-only notes when permitted (`tickets.internal_notes`) for team/admin visibility.
- **Attachments:** upload files with replies/notes to share evidence, screenshots, or documents.
- Activity is recorded for key actions to support traceability.

### Assignment and status updates
- **Assign/accept work:** update assignment fields when permitted (agents may be limited to self-assignment depending on permissions/configuration).
- **Update status/workflow:** move tickets through allowed workflow states (e.g. Open/In Progress/Pending/Resolved, etc.) when permitted.
- When resolving a ticket, provide a clear **resolution summary/note** as required by the workflow.

### Watchers and notifications
- **Watch/Unwatch** tickets to receive in-app updates.
- Check **Notifications** (`/notifications`) for new replies, status changes, assignment updates, and other relevant events.

### Team visibility
- Access **Teams** (`/teams`) if your role allows, to view team/department information used for routing and assignment.

## Best practices
- Acknowledge and respond promptly; first responses and status changes affect SLA tracking.
- Keep requester-facing replies clear and actionable.
- Use internal notes for sensitive/internal coordination details.
- Record meaningful resolution summaries so requesters and future agents understand the outcome.
- Watch tickets you are actively handling or need to monitor.

## Current limitations (may vary by deployment)
- Some administrative/configuration functions (user/role administration, catalog/SLA/business-hours management, audit/recycle-bin management, reports/exports, automated SLA jobs/escalations) may be limited to Team Leads/Admins or not yet fully implemented.