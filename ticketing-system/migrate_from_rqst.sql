-- Migration script: legacy `rqst` database -> `ticketing_system`
-- Source: rqst (tables: employees, offices, requests)
-- Target: ticketing_system (created by schema.sql)
--
-- IMPORTANT
-- 1) Run schema.sql first.
-- 2) Ensure the legacy database is available as `rqst` (or adjust source names below).
-- 3) Review mappings/defaults (priority, categories, roles, status mapping) before production use.
-- 4) Back up both databases before running.
-- 5) Re-run behavior: inserts use legacy traceability keys/unique constraints where possible.

USE `ticketing_system`;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;
-- Be tolerant of legacy zero dates/times during migration
SET @OLD_SQL_MODE := @@SESSION.sql_mode;
SET SESSION sql_mode = REPLACE(REPLACE(REPLACE(@@SESSION.sql_mode, 'NO_ZERO_DATE', ''), 'NO_ZERO_IN_DATE', ''), 'STRICT_TRANS_TABLES', '');

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1) Teams from offices
-- ---------------------------------------------------------------------------
INSERT INTO teams (legacy_office_id, name, code, is_active, is_service_team, created_at, updated_at)
SELECT
  o.id,
  COALESCE(NULLIF(TRIM(o.office), ''), CONCAT('Office #', o.id)),
  NULLIF(TRIM(o.office_code), ''),
  COALESCE(o.active, 1),
  COALESCE(o.isHandlingOffice, 0),
  NOW(),
  NOW()
FROM rqst.offices o
ON DUPLICATE KEY UPDATE
  name = VALUES(name),
  code = VALUES(code),
  is_active = VALUES(is_active),
  is_service_team = VALUES(is_service_team),
  updated_at = NOW();

-- Ensure a default service team exists for unmapped handling cases
INSERT INTO teams (legacy_office_id, name, code, is_active, is_service_team, created_at, updated_at)
SELECT NULL, 'Service Desk', 'SERVICE_DESK', 1, 1, NOW(), NOW()
WHERE NOT EXISTS (SELECT 1 FROM teams WHERE code = 'SERVICE_DESK');

-- ---------------------------------------------------------------------------
-- 2) Users from employees
-- ---------------------------------------------------------------------------
INSERT INTO users (
  legacy_employee_id, employee_number, username, email,
  first_name, middle_name, last_name, full_name,
  team_id, is_active, created_at, updated_at
)
SELECT
  e.employee_id,
  NULLIF(TRIM(e.employee_number), ''),
  NULLIF(TRIM(e.employee_number), ''),
  NULL, -- no email in legacy dump
  NULLIF(TRIM(e.first_name), ''),
  NULLIF(TRIM(e.middle_name), ''),
  NULLIF(TRIM(e.last_name), ''),
  TRIM(CONCAT_WS(' ',
      NULLIF(TRIM(e.first_name), ''),
      NULLIF(TRIM(e.middle_name), ''),
      NULLIF(TRIM(e.last_name), '')
  )),
  t.id,
  COALESCE(e.active, 1),
  NOW(),
  NOW()
FROM rqst.employees e
LEFT JOIN teams t ON t.legacy_office_id = e.office_id
ON DUPLICATE KEY UPDATE
  employee_number = VALUES(employee_number),
  first_name = VALUES(first_name),
  middle_name = VALUES(middle_name),
  last_name = VALUES(last_name),
  full_name = CASE WHEN VALUES(full_name) <> '' THEN VALUES(full_name) ELSE users.full_name END,
  team_id = VALUES(team_id),
  is_active = VALUES(is_active),
  updated_at = NOW();

-- Fallback full_name for any blank records
UPDATE users SET full_name = CONCAT('User #', id) WHERE full_name IS NULL OR TRIM(full_name) = '';

-- ---------------------------------------------------------------------------
-- 3) Role assignments (heuristic)
--    - requester: all users
--    - agent: users appearing as request personnel or in service/handling teams
--    - team_lead: office immediates/managers where identifiable
--    - admin: not auto-assigned (assign explicitly)
-- ---------------------------------------------------------------------------
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT u.id, r.id FROM users u JOIN roles r ON r.name = 'requester';

-- Agents: personnel assigned on requests
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT DISTINCT u.id, r.id
FROM rqst.requests req
JOIN users u ON u.legacy_employee_id IN (req.personnel1_id, req.personnel2_id, req.personnel3_id, req.personnel4_id)
JOIN roles r ON r.name = 'agent'
WHERE u.legacy_employee_id IS NOT NULL;

-- Agents: users in service/handling teams
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT DISTINCT u.id, r.id
FROM users u
JOIN teams t ON t.id = u.team_id AND t.is_service_team = 1
JOIN roles r ON r.name = 'agent';

-- Team leads: employees marked as office immediate (by immediate_id)
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT DISTINCT mgr.id, r.id
FROM rqst.offices o
JOIN users mgr ON mgr.legacy_employee_id = o.immediate_id
JOIN roles r ON r.name = 'team_lead'
WHERE o.immediate_id IS NOT NULL AND o.immediate_id <> 0;

-- Set team manager where known
UPDATE teams t
JOIN rqst.offices o ON o.id = t.legacy_office_id
JOIN users u ON u.legacy_employee_id = o.immediate_id
SET t.manager_user_id = u.id
WHERE o.immediate_id IS NOT NULL AND o.immediate_id <> 0;

-- ---------------------------------------------------------------------------
-- 4) Helper: map legacy status -> new status_id
-- ---------------------------------------------------------------------------
-- Mapping rationale:
--   created   -> new
--   assigned  -> in_progress (assigned/being handled)
--   completed -> resolved
--   rated     -> closed (completed and requester feedback recorded)
--   VOID/void -> canceled

-- ---------------------------------------------------------------------------
-- 5) Tickets from requests
-- ---------------------------------------------------------------------------
INSERT INTO tickets (
  legacy_request_id,
  ticket_number,
  requester_id,
  requester_team_id,
  requester_name,
  subject,
  description,
  category_id,
  priority_id,
  status_id,
  assigned_team_id,
  primary_assignee_id,
  source,
  noted_by_user_id,
  noted_by_name,
  approved_by_user_id,
  approved_by_name,
  created_at,
  updated_at,
  first_response_at,
  due_at,
  resolved_at,
  closed_at,
  last_activity_at,
  resolution_summary,
  turnaround_value,
  turnaround_unit,
  satisfaction_rating,
  satisfaction_comment,
  satisfaction_rated_at,
  is_archived,
  archived_at,
  cancel_reason,
  reopened_count,
  created_by,
  updated_by
)
SELECT
  r.request_id,
  COALESCE(NULLIF(TRIM(r.control_number), ''), CONCAT('REQ-', r.request_id)) AS ticket_number,
  req_user.id AS requester_id,
  req_team.id AS requester_team_id,
  NULLIF(TRIM(r.requester), '') AS requester_name,
  LEFT(COALESCE(NULLIF(TRIM(REPLACE(REPLACE(r.nature, '\r', ' '), '\n', ' ')), ''), CONCAT('Request ', COALESCE(NULLIF(TRIM(r.control_number), ''), r.request_id))), 255) AS subject,
  NULLIF(TRIM(r.nature), '') AS description,
  (SELECT id FROM ticket_categories WHERE name = 'General Service Request' ORDER BY id LIMIT 1) AS category_id,
  (SELECT id FROM ticket_priorities WHERE code = 'medium' LIMIT 1) AS priority_id,
  CASE LOWER(TRIM(COALESCE(r.status, 'created')))
    WHEN 'created'   THEN (SELECT id FROM ticket_statuses WHERE code = 'new' LIMIT 1)
    WHEN 'assigned'  THEN (SELECT id FROM ticket_statuses WHERE code = 'in_progress' LIMIT 1)
    WHEN 'completed' THEN (SELECT id FROM ticket_statuses WHERE code = 'resolved' LIMIT 1)
    WHEN 'rated'     THEN (SELECT id FROM ticket_statuses WHERE code = 'closed' LIMIT 1)
    WHEN 'void'      THEN (SELECT id FROM ticket_statuses WHERE code = 'canceled' LIMIT 1)
    ELSE (SELECT id FROM ticket_statuses WHERE code = 'new' LIMIT 1)
  END AS status_id,
  COALESCE(
    (SELECT t2.id FROM teams t2 WHERE t2.legacy_office_id = r.handling_office_id LIMIT 1),
    (SELECT t3.id FROM teams t3 WHERE t3.is_service_team = 1 ORDER BY t3.id LIMIT 1)
  ) AS assigned_team_id,
  pa.id AS primary_assignee_id,
  'portal' AS source,
  NULLIF(TRIM(r.noted_by_name), '') AS noted_by_name,
  nb_user.id AS noted_by_user_id,
  NULLIF(TRIM(r.approved_by_name), '') AS approved_by_name,
  ap_user.id AS approved_by_user_id,
  COALESCE(NULLIF(r.date_requested, '0000-00-00 00:00:00'), NOW()) AS created_at,
  COALESCE(
    NULLIF(r.date_archived, '0000-00-00 00:00:00'),
    NULLIF(r.date_completed, '0000-00-00 00:00:00'),
    NULLIF(r.date_received, '0000-00-00 00:00:00'),
    NULLIF(r.date_requested, '0000-00-00 00:00:00'),
    NOW()
  ) AS updated_at,
  NULLIF(r.date_received, '0000-00-00 00:00:00') AS first_response_at,
  NULL AS due_at,
  CASE WHEN LOWER(TRIM(COALESCE(r.status,''))) IN ('completed','rated')
       THEN COALESCE(NULLIF(r.date_completed, '0000-00-00 00:00:00'), NULLIF(r.date_archived, '0000-00-00 00:00:00'))
       ELSE NULLIF(r.date_completed, '0000-00-00 00:00:00') END AS resolved_at,
  CASE WHEN LOWER(TRIM(COALESCE(r.status,''))) = 'rated'
       THEN COALESCE(NULLIF(r.date_archived, '0000-00-00 00:00:00'), NULLIF(r.date_completed, '0000-00-00 00:00:00'))
       ELSE NULL END AS closed_at,
  COALESCE(
    NULLIF(r.date_archived, '0000-00-00 00:00:00'),
    NULLIF(r.date_completed, '0000-00-00 00:00:00'),
    NULLIF(r.date_received, '0000-00-00 00:00:00'),
    NULLIF(r.date_requested, '0000-00-00 00:00:00')
  ) AS last_activity_at,
  NULLIF(TRIM(r.action_taken), '') AS resolution_summary,
  r.turn_around_time AS turnaround_value,
  CASE WHEN r.turn_around_time IS NOT NULL THEN 'days' ELSE NULL END AS turnaround_unit,
  CASE WHEN r.rating IS NOT NULL AND r.rating > 0 THEN r.rating ELSE NULL END AS satisfaction_rating,
  NULLIF(TRIM(r.comment_suggestion), '') AS satisfaction_comment,
  CASE WHEN r.rating IS NOT NULL AND r.rating > 0
       THEN COALESCE(NULLIF(r.date_archived, '0000-00-00 00:00:00'), NULLIF(r.date_completed, '0000-00-00 00:00:00'))
       ELSE NULL END AS satisfaction_rated_at,
  COALESCE(r.archived, 0) AS is_archived,
  NULLIF(r.date_archived, '0000-00-00 00:00:00') AS archived_at,
  CASE WHEN LOWER(TRIM(COALESCE(r.status,''))) = 'void' THEN NULLIF(TRIM(r.remarks), '') ELSE NULL END AS cancel_reason,
  0 AS reopened_count,
  req_user.id AS created_by,
  COALESCE(ap_user.id, pa.id, req_user.id) AS updated_by
FROM rqst.requests r
LEFT JOIN users req_user ON req_user.legacy_employee_id = r.requested_by_id
LEFT JOIN teams req_team ON req_team.legacy_office_id = r.office_id
LEFT JOIN users nb_user  ON nb_user.legacy_employee_id = r.noted_by_id
LEFT JOIN users ap_user  ON ap_user.legacy_employee_id = r.approved_by_id
LEFT JOIN users pa ON pa.legacy_employee_id = COALESCE(
  NULLIF(r.personnel1_id, 0), NULLIF(r.personnel2_id, 0), NULLIF(r.personnel3_id, 0), NULLIF(r.personnel4_id, 0)
)
ON DUPLICATE KEY UPDATE
  requester_id = VALUES(requester_id),
  requester_team_id = VALUES(requester_team_id),
  requester_name = VALUES(requester_name),
  subject = VALUES(subject),
  description = VALUES(description),
  status_id = VALUES(status_id),
  assigned_team_id = VALUES(assigned_team_id),
  primary_assignee_id = VALUES(primary_assignee_id),
  noted_by_user_id = VALUES(noted_by_user_id),
  noted_by_name = VALUES(noted_by_name),
  approved_by_user_id = VALUES(approved_by_user_id),
  approved_by_name = VALUES(approved_by_name),
  updated_at = VALUES(updated_at),
  first_response_at = VALUES(first_response_at),
  resolved_at = VALUES(resolved_at),
  closed_at = VALUES(closed_at),
  last_activity_at = VALUES(last_activity_at),
  resolution_summary = VALUES(resolution_summary),
  turnaround_value = VALUES(turnaround_value),
  turnaround_unit = VALUES(turnaround_unit),
  satisfaction_rating = VALUES(satisfaction_rating),
  satisfaction_comment = VALUES(satisfaction_comment),
  satisfaction_rated_at = VALUES(satisfaction_rated_at),
  is_archived = VALUES(is_archived),
  archived_at = VALUES(archived_at),
  cancel_reason = VALUES(cancel_reason),
  updated_by = VALUES(updated_by);

-- ---------------------------------------------------------------------------
-- 6) Ticket assignees (all personnel slots)
-- ---------------------------------------------------------------------------
INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at, assigned_by)
SELECT t.id, u.id, 1,
       COALESCE(t.first_response_at, t.created_at),
       t.approved_by_user_id
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = r.personnel1_id
WHERE r.personnel1_id IS NOT NULL AND r.personnel1_id <> 0;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at, assigned_by)
SELECT t.id, u.id, CASE WHEN r.personnel1_id IS NULL OR r.personnel1_id = 0 THEN 1 ELSE 0 END,
       COALESCE(t.first_response_at, t.created_at),
       t.approved_by_user_id
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = r.personnel2_id
WHERE r.personnel2_id IS NOT NULL AND r.personnel2_id <> 0;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at, assigned_by)
SELECT t.id, u.id, 0,
       COALESCE(t.first_response_at, t.created_at),
       t.approved_by_user_id
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = r.personnel3_id
WHERE r.personnel3_id IS NOT NULL AND r.personnel3_id <> 0;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at, assigned_by)
SELECT t.id, u.id, 0,
       COALESCE(t.first_response_at, t.created_at),
       t.approved_by_user_id
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = r.personnel4_id
WHERE r.personnel4_id IS NOT NULL AND r.personnel4_id <> 0;

-- ---------------------------------------------------------------------------
-- 7) Preserve legacy resolution/remarks/feedback as comments
-- ---------------------------------------------------------------------------
INSERT INTO ticket_comments (ticket_id, author_id, author_name, body, is_internal, comment_type, created_at)
SELECT t.id, t.primary_assignee_id, NULL,
       CONCAT('Action taken: ', TRIM(r.action_taken)),
       1, 'resolution',
       COALESCE(NULLIF(r.date_completed, '0000-00-00 00:00:00'), t.updated_at, t.created_at)
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
WHERE r.action_taken IS NOT NULL AND TRIM(r.action_taken) <> ''
  AND NOT EXISTS (
    SELECT 1 FROM ticket_comments c
    WHERE c.ticket_id = t.id AND c.comment_type = 'resolution' AND c.body = CONCAT('Action taken: ', TRIM(r.action_taken))
  );

INSERT INTO ticket_comments (ticket_id, author_id, author_name, body, is_internal, comment_type, created_at)
SELECT t.id, t.requester_id, t.requester_name,
       CONCAT('Feedback', CASE WHEN r.rating IS NOT NULL AND r.rating > 0 THEN CONCAT(' (rating: ', r.rating, '/5)') ELSE '' END,
              ': ', TRIM(r.comment_suggestion)),
       0, 'feedback',
       COALESCE(t.satisfaction_rated_at, t.closed_at, t.resolved_at, t.updated_at, t.created_at)
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
WHERE r.comment_suggestion IS NOT NULL AND TRIM(r.comment_suggestion) <> ''
  AND NOT EXISTS (
    SELECT 1 FROM ticket_comments c
    WHERE c.ticket_id = t.id AND c.comment_type = 'feedback' AND c.body LIKE CONCAT('Feedback%', TRIM(r.comment_suggestion), '%')
  );

INSERT INTO ticket_comments (ticket_id, author_id, author_name, body, is_internal, comment_type, created_at)
SELECT t.id, t.updated_by, NULL,
       CONCAT('Void/remarks: ', TRIM(r.remarks)),
       1, 'system',
       COALESCE(t.updated_at, t.created_at)
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
WHERE r.remarks IS NOT NULL AND TRIM(r.remarks) <> ''
  AND NOT EXISTS (
    SELECT 1 FROM ticket_comments c
    WHERE c.ticket_id = t.id AND c.comment_type = 'system' AND c.body = CONCAT('Void/remarks: ', TRIM(r.remarks))
  );

-- ---------------------------------------------------------------------------
-- 8) Initial activity log entries
-- ---------------------------------------------------------------------------
INSERT INTO ticket_activity_logs (ticket_id, actor_id, actor_name, event_type, field_name, old_value, new_value, metadata, created_at)
SELECT t.id, t.created_by, t.requester_name, 'created', 'status', NULL, ts.code,
       JSON_OBJECT('legacy_request_id', t.legacy_request_id, 'legacy_status', r.status),
       t.created_at
FROM tickets t
JOIN rqst.requests r ON r.request_id = t.legacy_request_id
JOIN ticket_statuses ts ON ts.id = t.status_id
WHERE NOT EXISTS (
  SELECT 1 FROM ticket_activity_logs a WHERE a.ticket_id = t.id AND a.event_type = 'created'
);

INSERT INTO ticket_activity_logs (ticket_id, actor_id, actor_name, event_type, field_name, old_value, new_value, metadata, created_at)
SELECT t.id, ta.user_id, u.full_name, 'assigned', 'assignee', NULL, u.full_name,
       JSON_OBJECT('is_primary', ta.is_primary),
       ta.assigned_at
FROM ticket_assignees ta
JOIN tickets t ON t.id = ta.ticket_id
JOIN users u ON u.id = ta.user_id
WHERE NOT EXISTS (
  SELECT 1 FROM ticket_activity_logs a
  WHERE a.ticket_id = ta.ticket_id AND a.event_type = 'assigned' AND a.actor_id = ta.user_id AND a.new_value = u.full_name
);

-- ---------------------------------------------------------------------------
-- 9) Migration summary (result sets)
-- ---------------------------------------------------------------------------
SELECT 'teams' AS entity, COUNT(*) AS migrated_rows FROM teams
UNION ALL SELECT 'users', COUNT(*) FROM users
UNION ALL SELECT 'user_roles', COUNT(*) FROM user_roles
UNION ALL SELECT 'tickets', COUNT(*) FROM tickets
UNION ALL SELECT 'ticket_assignees', COUNT(*) FROM ticket_assignees
UNION ALL SELECT 'ticket_comments', COUNT(*) FROM ticket_comments
UNION ALL SELECT 'ticket_activity_logs', COUNT(*) FROM ticket_activity_logs;

SELECT ts.code AS status, COUNT(*) AS tickets
FROM tickets t JOIN ticket_statuses ts ON ts.id = t.status_id
GROUP BY ts.code ORDER BY tickets DESC;

COMMIT;
SET SESSION sql_mode = @OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS = 1;
