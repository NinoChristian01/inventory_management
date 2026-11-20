# Industry-Standard Ticketing System (Optimized from `rqst`)

This folder contains an industry-standard, normalized ticketing system design and migration assets derived from the legacy `rqst` database (`rqst.sql`).

## Contents
| File | Purpose |
|---|---|
| `schema.sql` | Creates the target `ticketing_system` database/schema with normalized tables, constraints, indexes, seed data, and a queue view. |
| `migrate_from_rqst.sql` | Migrates data from legacy `rqst` (`offices`, `employees`, `requests`) into `ticketing_system` with status/role/assignee mapping and preserved history. |
| `README.md` | Design rationale, workflow, deployment, and migration guidance. |

## 1. Optimal Solution Overview

The legacy `rqst` system tracked service requests in a single wide `requests` table with denormalized requester/assignee names, limited status values (`created`, `assigned`, `completed`, `rated`, `VOID`), and no dedicated comment/activity/attachment/SLA structures. The optimized solution adopts common ITSM/help-desk practices:

- **Normalized core model:** separate users, teams, tickets, assignees, comments, activity logs, attachments, and lookup/config tables.
- **Standard lifecycle:** explicit statuses covering intake, active work, waiting states, resolution, closure, cancellation, and reopen.
- **Prioritization & SLA readiness:** priority levels with target response/resolution times and an `sla_policies` table.
- **Accountability & traceability:** immutable-style activity log, timestamped comments (public/internal/system), assignment history, and legacy ID traceability (`legacy_*` fields).
- **Search & queue performance:** indexes on status/priority/team/assignee/dates/ticket number and full-text index on subject/description.
- **Extensibility:** categories, configurable lookups, attachment metadata, and fields for integration/auth expansion.

## 2. Target Data Model

### Core entities
- **`users`**: requesters, agents, leads, admins (mapped from legacy employees; supports future auth/profile fields).
- **`roles` / `user_roles`**: RBAC assignments (`requester`, `agent`, `team_lead`, `admin`).
- **`teams`**: organizational/service groups (mapped from legacy offices).
- **`tickets`**: primary ticket record (identity, requester, classification, status, ownership, timestamps, SLA/resolution/feedback, archive/cancel metadata).
- **`ticket_assignees`**: supports one or multiple assignees with primary-assignee flag (replaces fixed `personnel1-4` slots).
- **`ticket_comments`**: public replies, internal notes, resolution notes, and migrated feedback/remarks.
- **`ticket_activity_logs`**: audit trail of creation, assignment, status/field changes, and other events.
- **`ticket_attachments`**: attachment metadata linked to tickets/comments.
- **Lookups/config:** `ticket_statuses`, `ticket_priorities`, `ticket_categories`, `sla_policies`.
- **View:** `vw_ticket_queue` for common queue/reporting reads.

### Why this is industry-standard
- Separates **current state** (`tickets`) from **conversation/history** (`ticket_comments`, `ticket_activity_logs`).
- Models **ownership** explicitly (team + primary assignee + additional assignees/watchers).
- Enables **SLA measurement** via priority/policy targets and key timestamps (`created_at`, `first_response_at`, `due_at`, `resolved_at`, `closed_at`).
- Preserves **auditability** and supports concurrent operations/reporting at scale better than a single denormalized table.

## 3. Status, Priority, and Workflow

### Seeded statuses
| Code | Meaning | Open | Terminal |
|---|---|---:|---:|
| `new` | Logged, awaiting triage | Yes | No |
| `open` | Accepted and queued for work | Yes | No |
| `in_progress` | Actively being worked | Yes | No |
| `pending_requester` | Waiting for requester input/action | Yes | No |
| `pending_third_party` | Waiting for external/vendor dependency | Yes | No |
| `on_hold` | Paused intentionally | Yes | No |
| `reopened` | Returned to active handling after resolution/closure | Yes | No |
| `resolved` | Solution provided; pending confirmation/closure | Yes | No |
| `closed` | Finalized/completed | No | Yes |
| `canceled` | Withdrawn/invalid/not to be worked | No | Yes |

### Recommended lifecycle
1. **Intake (`new/open`)**: validate request, categorize, set priority/SLA, route to team.
2. **Assignment (`in_progress`)**: assign primary agent/collaborators and begin work.
3. **Waiting states (`pending_requester`, `pending_third_party`, `on_hold`)**: record blockers; track SLA policy (pause/resume) per business rules.
4. **Resolution (`resolved`)**: document action taken and notify requester for confirmation.
5. **Closure (`closed`)**: requester confirms or auto-close after grace period; capture satisfaction.
6. **Exception paths:** `canceled` for void/invalid requests; `reopened` when issue recurs or confirmation fails.

### Seeded priorities (defaults)
| Priority | Typical response target | Typical resolution target |
|---|---:|---:|
| Low | 24 hours | 5 days |
| Medium | 8 hours | 2 days |
| High | 2 hours | 1 day |
| Critical | 30 minutes | 8 hours |

> Adjust targets to your organization’s SLA policy and business hours.

## 4. Roles and Permissions (RBAC baseline)

| Capability | Requester | Agent | Team Lead | Admin |
|---|---:|---:|---:|---:|
| Create ticket | Own | Yes (on behalf) | Yes | Yes |
| View tickets | Own scope | Team/assigned scope | Team/org scope | All |
| Comment/reply | Public on own | Public/internal | Public/internal | Public/internal |
| Update status/category/notes | Limited | Yes (workflow-governed) | Yes | Yes |
| Assign/reassign | No | Limited/self (policy) | Yes | Yes |
| Set/change priority & SLA | No/request | Limited | Yes | Yes |
| Resolve/close/reopen | Confirm/reopen own (policy) | Yes | Yes | Yes |
| Configure users/teams/categories/SLA/workflows | No | No | Limited | Yes |
| View audit/activity logs | Limited | Scoped | Yes | Yes |

Enforce permissions in the application/API layer using `roles`/`user_roles` and team/ownership attributes.

## 5. Migration from `rqst`

### Legacy → target mapping
| Legacy (`rqst`) | Target (`ticketing_system`) | Notes |
|---|---|---|
| `offices` | `teams` | Preserves `legacy_office_id`, code/name, active/service flags. |
| `employees` | `users` (+ `user_roles`) | Preserves `legacy_employee_id`; roles assigned heuristically (see script). |
| `requests.request_id/control_number` | `tickets.legacy_request_id/ticket_number` | Traceable, unique ticket identity. |
| `requested_by_id/requester/office_id` | `tickets.requester_id/requester_name/requester_team_id` | Requester snapshot retained. |
| `nature` | `tickets.subject` (derived) + `tickets.description` (full text) | Subject trimmed to 255 chars. |
| `personnel1-4` + IDs | `ticket_assignees`, `tickets.primary_assignee_id` | Normalized multi-assignee model. |
| `date_requested/received/completed/archived` | `created_at/first_response_at/resolved_at/closed_at/archived_at` | Zero-dates normalized to NULL where applicable. |
| `status` | `ticket_statuses` | `created→new`, `assigned→in_progress`, `completed→resolved`, `rated→closed`, `VOID/void→canceled`. |
| `action_taken` | `resolution_summary` + resolution `ticket_comments` | Preserves resolution history. |
| `rating/comment_suggestion` | `satisfaction_rating/comment` + feedback `ticket_comments` | Preserves CSAT data. |
| `remarks` (void/cancel) | `cancel_reason` + system `ticket_comments` | Preserves void rationale. |
| `noted_by_*`, `approved_by_*`, `turn_around_time`, `archived` | corresponding ticket fields | Carried forward for audit/reporting. |
| (generated) | `ticket_activity_logs` | Creation and assignment events backfilled. |

### Deployment & migration steps
1. **Backup:** back up the existing `rqst` database and application.
2. **Provision target schema:** run `schema.sql` on your MySQL/MariaDB server.
   ```sql
   SOURCE ticketing-system/schema.sql;
   ```
3. **Load/ensure legacy source:** ensure the legacy database is accessible as `rqst` (import `rqst.sql` if needed).
   ```sql
   SOURCE rqst.sql;
   ```
4. **Run migration:** execute the migration script.
   ```sql
   SOURCE ticketing-system/migrate_from_rqst.sql;
   ```
5. **Validate:** review migration summary result sets and spot-check counts/status distribution against legacy data.
   ```sql
   -- Example checks
   SELECT COUNT(*) FROM rqst.requests;
   SELECT COUNT(*) FROM ticketing_system.tickets;
   SELECT ts.code, COUNT(*) FROM ticketing_system.tickets t
   JOIN ticketing_system.ticket_statuses ts ON ts.id = t.status_id
   GROUP BY ts.code;
   ```
6. **Post-migration configuration:**
   - Review auto-assigned roles (`user_roles`) and grant `admin`/`team_lead` explicitly as needed.
   - Refine `ticket_categories`, `sla_policies`, and priority targets to match operations.
   - Populate user emails/auth identifiers and integrate with your authentication/SSO.
   - Configure notification, auto-close, and reopen-window policies in the application layer.

## 6. Operational, Scalability, and UX Recommendations

- **Concurrency/integrity:** use transactional updates and optimistic concurrency (`updated_at`/version checks) for ticket edits; keep activity logs append-only.
- **Performance:** rely on provided indexes for queue filters; paginate all lists; use full-text search for subject/description (and consider external search for comment bodies at scale).
- **SLA automation:** run scheduled jobs/workers to compute `due_at`, detect breaches, send reminders, and auto-close resolved tickets per policy.
- **Data quality:** enforce required fields and controlled vocabularies (status transitions, categories, priorities) at the application/service layer.
- **Security:** server-side RBAC/ABAC, least-privilege access, audit visibility for privileged actions, and secure attachment storage/scanning.
- **UX:** role-specific dashboards/queues, saved filters, clear status definitions, collision warnings on concurrent edits, canned responses, and transparent SLA/next-step indicators.
- **Evolution:** add custom fields, knowledge-base linkage, omnichannel intake, and reporting/analytics as needed; archive old closed tickets according to retention policy.

## 7. Notes and Assumptions
- Legacy data is preserved via `legacy_*` traceability fields and migrated comments/activity for key fields (action taken, feedback, void remarks).
- Default migrated priority is **Medium** and category is **General Service Request** because legacy records do not explicitly encode priority/category; refine during or after migration.
- Role assignment is heuristic (based on personnel/service-team/immediate relationships); verify and adjust for least privilege.
- The schema targets MySQL/MariaDB; adapt types/indexing/features if deploying on another RDBMS.