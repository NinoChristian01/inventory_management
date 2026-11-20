-- Phase 2: seed reference data and import legacy rqst data into EF-generated schema
-- Source (read-only): rqst.offices, rqst.employees, rqst.requests
-- Target: selected database (tables created by EF InitialCreate), e.g. ticketing_system / ticketing_system_test
-- Idempotent-ish: uses INSERT ... ON DUPLICATE KEY UPDATE / INSERT IGNORE where practical.

SET NAMES utf8mb4;
SET @OLD_FOREIGN_KEY_CHECKS := @@FOREIGN_KEY_CHECKS;
SET FOREIGN_KEY_CHECKS = 0;
SET @OLD_SQL_MODE := @@SESSION.sql_mode;
SET SESSION sql_mode = REPLACE(REPLACE(REPLACE(@@SESSION.sql_mode, 'NO_ZERO_DATE', ''), 'NO_ZERO_IN_DATE', ''), 'STRICT_TRANS_TABLES', '');

START TRANSACTION;

-- ---------------------------------------------------------------------------
-- 1) Reference data: statuses, priorities, roles, category, business hours, SLA, settings
-- ---------------------------------------------------------------------------
INSERT INTO ticket_statuses (id, code, name, is_open, is_terminal, pauses_sla, sort_order) VALUES
  (1,  'New',               'New',                1, 0, 0, 10),
  (2,  'Open',              'Open',               1, 0, 0, 20),
  (3,  'InProgress',        'In Progress',        1, 0, 0, 30),
  (4,  'PendingRequester',  'Pending Requester',  1, 0, 1, 40),
  (5,  'PendingThirdParty', 'Pending Third Party',1, 0, 1, 50),
  (6,  'OnHold',            'On Hold',            1, 0, 1, 60),
  (7,  'Reopened',          'Reopened',           1, 0, 0, 25),
  (8,  'Resolved',          'Resolved',           1, 0, 0, 70),
  (9,  'Closed',            'Closed',             0, 1, 0, 80),
  (10, 'Canceled',          'Canceled',           0, 1, 0, 90)
ON DUPLICATE KEY UPDATE
  code=VALUES(code), name=VALUES(name), is_open=VALUES(is_open), is_terminal=VALUES(is_terminal),
  pauses_sla=VALUES(pauses_sla), sort_order=VALUES(sort_order);

INSERT INTO ticket_priorities (id, code, name, color, sort_order) VALUES
  (1, 'Low',      'Low',      '#2E7D32', 10),
  (2, 'Medium',   'Medium',   '#1976D2', 20),
  (3, 'High',     'High',     '#F57C00', 30),
  (4, 'Critical', 'Critical', '#C62828', 40)
ON DUPLICATE KEY UPDATE
  code=VALUES(code), name=VALUES(name), color=VALUES(color), sort_order=VALUES(sort_order);

INSERT INTO roles (id, is_system, created_at_utc, updated_at_utc, name, normalized_name, concurrency_stamp) VALUES
  (1, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'Requester', 'REQUESTER', UUID()),
  (2, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'Agent',     'AGENT',     UUID()),
  (3, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'TeamLead',  'TEAMLEAD',  UUID()),
  (4, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6), 'Admin',     'ADMIN',     UUID())
ON DUPLICATE KEY UPDATE
  is_system=VALUES(is_system), name=VALUES(name), normalized_name=VALUES(normalized_name), updated_at_utc=UTC_TIMESTAMP(6);

INSERT INTO ticket_categories (id, name, parent_id, default_team_id, is_active, is_deleted, created_at_utc, updated_at_utc)
SELECT 1, 'General Service Request', NULL, NULL, 1, 0, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
WHERE NOT EXISTS (SELECT 1 FROM ticket_categories WHERE id = 1);

-- Business hours: Mon-Fri 08:00-12:00, 13:00-17:00 (.NET DayOfWeek: Sun=0, Mon=1..Fri=5, Sat=6)
DELETE FROM business_hours;
INSERT INTO business_hours (day_of_week, start_time, end_time, is_active) VALUES
  (1, '08:00:00', '12:00:00', 1), (1, '13:00:00', '17:00:00', 1),
  (2, '08:00:00', '12:00:00', 1), (2, '13:00:00', '17:00:00', 1),
  (3, '08:00:00', '12:00:00', 1), (3, '13:00:00', '17:00:00', 1),
  (4, '08:00:00', '12:00:00', 1), (4, '13:00:00', '17:00:00', 1),
  (5, '08:00:00', '12:00:00', 1), (5, '13:00:00', '17:00:00', 1);

-- SLA policies (default priority-based). 1 business day = 8 business hours = 480 minutes.
DELETE FROM sla_policies;
INSERT INTO sla_policies (priority_id, category_id, response_target_minutes, resolution_target_minutes, use_business_hours, warning_percent, is_active, created_at_utc, updated_at_utc) VALUES
  (1, NULL, 1440, 2400, 1, 75, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)), -- Low: 24bh / 5bd
  (2, NULL,  480,  960, 1, 75, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)), -- Medium: 8bh / 2bd
  (3, NULL,  120,  480, 1, 75, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)), -- High: 2bh / 1bd
  (4, NULL,   30,  480, 0, 75, 1, UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)); -- Critical: 30m / 8h 24x7

INSERT INTO app_settings (setting_key, setting_value, updated_at_utc) VALUES
  ('auto_close_resolved_days', '3', UTC_TIMESTAMP(6)),
  ('sla_monitor_interval_seconds', '60', UTC_TIMESTAMP(6)),
  ('legacy_timezone', 'Asia/Manila', UTC_TIMESTAMP(6)),
  ('legacy_sla_excluded', 'true', UTC_TIMESTAMP(6))
ON DUPLICATE KEY UPDATE setting_value=VALUES(setting_value), updated_at_utc=UTC_TIMESTAMP(6);

-- ---------------------------------------------------------------------------
-- 2) Teams from legacy offices (65)
-- ---------------------------------------------------------------------------
INSERT INTO teams (id, code, name, is_service_team, is_active, is_deleted, manager_user_id, legacy_office_id, created_at_utc, updated_at_utc)
SELECT
  o.id,
  CASE
    WHEN o.office_code IS NULL OR TRIM(o.office_code) = '' THEN CONCAT('OFF-', o.id)
    WHEN o.office_code = 'ETEEAP' AND o.id <> (SELECT MIN(o2.id) FROM rqst.offices o2 WHERE o2.office_code='ETEEAP')
      THEN CONCAT('ETEEAP-', o.id)
    ELSE TRIM(o.office_code)
  END,
  COALESCE(NULLIF(TRIM(o.office), ''), CONCAT('Office #', o.id)),
  CASE WHEN COALESCE(o.isHandlingOffice, 0) = 1 OR UPPER(TRIM(COALESCE(o.office_code,''))) = 'MIS' THEN 1 ELSE 0 END,
  COALESCE(o.active, 1),
  0,
  NULL,
  o.id,
  UTC_TIMESTAMP(6), UTC_TIMESTAMP(6)
FROM rqst.offices o
ON DUPLICATE KEY UPDATE
  code=VALUES(code), name=VALUES(name), is_service_team=VALUES(is_service_team),
  is_active=VALUES(is_active), updated_at_utc=UTC_TIMESTAMP(6);

SET @mis_team_id := (SELECT id FROM teams WHERE UPPER(code)='MIS' ORDER BY id LIMIT 1);
UPDATE ticket_categories SET default_team_id = @mis_team_id WHERE id = 1 AND @mis_team_id IS NOT NULL;

-- ---------------------------------------------------------------------------
-- 3) Users from legacy employees (788)
-- ---------------------------------------------------------------------------
INSERT INTO users (
  id, employee_number, last_name, first_name, middle_name, suffix, full_name,
  department_id, is_active, must_change_password, legacy_employee_id,
  created_at_utc, updated_at_utc,
  user_name, normalized_user_name, email, normalized_email, email_confirmed,
  password_hash, security_stamp, concurrency_stamp, phone_number, phone_number_confirmed,
  two_factor_enabled, lockout_end, lockout_enabled, access_failed_count
)
SELECT
  e.employee_id,
  CASE
    WHEN e.employee_number IS NULL OR TRIM(e.employee_number) = '' OR TRIM(e.employee_number) = '0' THEN NULL
    ELSE TRIM(e.employee_number)
  END,
  COALESCE(NULLIF(TRIM(e.last_name), ''), '(Unknown)'),
  COALESCE(NULLIF(TRIM(e.first_name), ''), '(Unknown)'),
  CASE WHEN e.middle_name IS NULL OR TRIM(e.middle_name) = '' THEN NULL ELSE TRIM(e.middle_name) END,
  NULL,
  CASE
    WHEN TRIM(CONCAT_WS(' ', NULLIF(TRIM(e.first_name),''), NULLIF(TRIM(e.middle_name),''), NULLIF(TRIM(e.last_name),''))) = ''
      THEN CONCAT('Employee #', e.employee_id)
    ELSE TRIM(CONCAT_WS(' ', NULLIF(TRIM(e.first_name),''), NULLIF(TRIM(e.middle_name),''), NULLIF(TRIM(e.last_name),'')))
  END,
  t.id,
  COALESCE(e.active, 1),
  1,
  e.employee_id,
  UTC_TIMESTAMP(6), UTC_TIMESTAMP(6),
  CASE
    WHEN e.employee_number IS NULL OR TRIM(e.employee_number) = '' OR TRIM(e.employee_number) = '0' THEN CONCAT('emp-', e.employee_id)
    ELSE TRIM(e.employee_number)
  END,
  UPPER(CASE
    WHEN e.employee_number IS NULL OR TRIM(e.employee_number) = '' OR TRIM(e.employee_number) = '0' THEN CONCAT('emp-', e.employee_id)
    ELSE TRIM(e.employee_number)
  END),
  NULL, NULL, 0,
  NULL, UUID(), UUID(), NULL, 0,
  0, NULL, 1, 0
FROM rqst.employees e
LEFT JOIN teams t ON t.legacy_office_id = e.office_id
ON DUPLICATE KEY UPDATE
  employee_number=VALUES(employee_number), last_name=VALUES(last_name), first_name=VALUES(first_name),
  middle_name=VALUES(middle_name), full_name=VALUES(full_name), department_id=VALUES(department_id),
  is_active=VALUES(is_active), legacy_employee_id=VALUES(legacy_employee_id),
  user_name=VALUES(user_name), normalized_user_name=VALUES(normalized_user_name), updated_at_utc=UTC_TIMESTAMP(6);

-- Resolve normalized_user_name collisions (keep first by legacy_employee_id, fallback others to emp-<id>)
UPDATE users u
JOIN (
  SELECT normalized_user_name
  FROM users
  WHERE legacy_employee_id IS NOT NULL
  GROUP BY normalized_user_name
  HAVING COUNT(*) > 1
) d ON d.normalized_user_name = u.normalized_user_name
JOIN (
  SELECT normalized_user_name, MIN(legacy_employee_id) AS keep_legacy_id
  FROM users
  WHERE legacy_employee_id IS NOT NULL
  GROUP BY normalized_user_name
  HAVING COUNT(*) > 1
) k ON k.normalized_user_name = u.normalized_user_name
SET u.user_name = CONCAT('emp-', u.legacy_employee_id),
    u.normalized_user_name = UPPER(CONCAT('emp-', u.legacy_employee_id)),
    u.employee_number = NULL
WHERE u.legacy_employee_id <> k.keep_legacy_id;

-- Requester role for imported users
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT u.id, 1 FROM users u WHERE u.legacy_employee_id IS NOT NULL;

-- Team managers / leads from office immediate
UPDATE teams t
JOIN rqst.offices o ON o.id = t.legacy_office_id
JOIN users u ON u.legacy_employee_id = o.immediate_id
SET t.manager_user_id = u.id
WHERE o.immediate_id IS NOT NULL AND o.immediate_id <> 0;

INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT DISTINCT u.id, 3
FROM rqst.offices o
JOIN users u ON u.legacy_employee_id = o.immediate_id
WHERE o.immediate_id IS NOT NULL AND o.immediate_id <> 0;

-- ---------------------------------------------------------------------------
-- 5) Tickets from legacy requests (1124)
-- ---------------------------------------------------------------------------
DROP TEMPORARY TABLE IF EXISTS tmp_rqst_requests;
CREATE TEMPORARY TABLE tmp_rqst_requests AS
SELECT
  r.*,
  NULLIF(r.date_requested, '0000-00-00 00:00:00') AS req_dt,
  NULLIF(r.date_received,  '0000-00-00 00:00:00') AS recv_dt,
  NULLIF(r.date_completed, '0000-00-00 00:00:00') AS comp_dt,
  NULLIF(r.date_archived,  '0000-00-00 00:00:00') AS arch_dt
FROM rqst.requests r;

INSERT INTO tickets (
  id, ticket_number, requester_id, requester_department_id, requester_name_snapshot,
  subject, description,
  status_id, priority_id, category_id, assigned_team_id, primary_assignee_id,
  created_at_utc, updated_at_utc, first_response_at_utc, response_due_at_utc, resolution_due_at_utc,
  resolved_at_utc, closed_at_utc, canceled_at_utc, last_reopened_at_utc,
  resolved_by_id, closed_by_id, close_reason, canceled_by_id, cancel_reason,
  resolution_code, resolution_summary,
  satisfaction_rating, satisfaction_comment, satisfaction_rated_at_utc,
  legacy_request_id, legacy_noted_by_id, legacy_noted_by_name, legacy_approved_by_id, legacy_approved_by_name,
  legacy_turnaround_value, legacy_turnaround_unit, legacy_is_archived, legacy_archived_at_utc,
  version
)
SELECT
  x.request_id,
  COALESCE(NULLIF(TRIM(x.control_number), ''), CONCAT('REQ-', x.request_id)),
  req_u.id,
  req_team.id,
  NULLIF(TRIM(x.requester), ''),
  LEFT(
    COALESCE(
      NULLIF(TRIM(REGEXP_REPLACE(COALESCE(x.nature,''), '[[:space:]]+', ' ')), ''),
      CONCAT('Request ', COALESCE(NULLIF(TRIM(x.control_number), ''), x.request_id))
    ), 255
  ) AS subject_txt,
  CASE
    WHEN x.nature IS NULL OR TRIM(x.nature) = '' THEN CONCAT('Legacy request ', COALESCE(NULLIF(TRIM(x.control_number), ''), x.request_id), ' (no description provided).')
    ELSE x.nature
  END AS description_txt,
  CASE LOWER(TRIM(COALESCE(x.status,'')))
    WHEN 'created'   THEN 1
    WHEN 'assigned'  THEN 3
    WHEN 'completed' THEN 8
    WHEN 'rated'     THEN 9
    WHEN 'void'      THEN 10
    ELSE 1
  END AS status_id_mapped,
  2 AS priority_id_mapped,
  1 AS category_id_mapped,
  @mis_team_id AS assigned_team_mis,
  pa_u.id AS primary_assignee_id_mapped,
  /* created_at_utc (legacy local UTC+8 -> UTC) */
  DATE_SUB(COALESCE(x.req_dt, UTC_TIMESTAMP()), INTERVAL 8 HOUR) AS created_utc,
  /* updated_at_utc */
  DATE_SUB(COALESCE(x.arch_dt, x.comp_dt, x.recv_dt, x.req_dt, UTC_TIMESTAMP()), INTERVAL 8 HOUR) AS updated_utc,
  /* first_response_at_utc: clamp received < requested to requested */
  CASE
    WHEN x.recv_dt IS NULL THEN NULL
    WHEN x.req_dt IS NOT NULL AND x.recv_dt < x.req_dt THEN DATE_SUB(x.req_dt, INTERVAL 8 HOUR)
    ELSE DATE_SUB(x.recv_dt, INTERVAL 8 HOUR)
  END AS first_response_utc,
  NULL, NULL,
  /* resolved_at_utc */
  CASE
    WHEN LOWER(TRIM(COALESCE(x.status,''))) IN ('completed','rated') THEN
      DATE_SUB(COALESCE(
        CASE WHEN x.comp_dt IS NOT NULL AND x.req_dt IS NOT NULL AND x.comp_dt < x.req_dt THEN x.req_dt ELSE x.comp_dt END,
        x.arch_dt, x.req_dt
      ), INTERVAL 8 HOUR)
    WHEN x.comp_dt IS NOT NULL THEN
      DATE_SUB(CASE WHEN x.req_dt IS NOT NULL AND x.comp_dt < x.req_dt THEN x.req_dt ELSE x.comp_dt END, INTERVAL 8 HOUR)
    ELSE NULL
  END AS resolved_utc,
  /* closed_at_utc */
  CASE
    WHEN LOWER(TRIM(COALESCE(x.status,''))) = 'rated' THEN
      DATE_SUB(COALESCE(x.arch_dt,
        CASE WHEN x.comp_dt IS NOT NULL AND x.req_dt IS NOT NULL AND x.comp_dt < x.req_dt THEN x.req_dt ELSE x.comp_dt END,
        x.req_dt
      ), INTERVAL 8 HOUR)
    ELSE NULL
  END AS closed_utc,
  /* canceled_at_utc */
  CASE
    WHEN LOWER(TRIM(COALESCE(x.status,''))) = 'void' THEN DATE_SUB(COALESCE(x.arch_dt, x.comp_dt, x.req_dt, UTC_TIMESTAMP()), INTERVAL 8 HOUR)
    ELSE NULL
  END AS canceled_utc,
  NULL,
  CASE WHEN LOWER(TRIM(COALESCE(x.status,''))) IN ('completed','rated') THEN pa_u.id ELSE NULL END,
  CASE WHEN LOWER(TRIM(COALESCE(x.status,''))) = 'rated' THEN ap_u.id ELSE NULL END,
  NULL,
  CASE WHEN LOWER(TRIM(COALESCE(x.status,''))) = 'void' THEN ap_u.id ELSE NULL END,
  CASE WHEN LOWER(TRIM(COALESCE(x.status,''))) = 'void' THEN NULLIF(TRIM(x.remarks), '') ELSE NULL END,
  NULL,
  NULLIF(TRIM(x.action_taken), ''),
  CASE WHEN x.rating IS NOT NULL AND x.rating > 0 THEN x.rating ELSE NULL END,
  NULLIF(TRIM(x.comment_suggestion), ''),
  CASE WHEN x.rating IS NOT NULL AND x.rating > 0 THEN DATE_SUB(COALESCE(x.arch_dt, x.comp_dt, x.req_dt), INTERVAL 8 HOUR) ELSE NULL END,
  x.request_id,
  NULLIF(x.noted_by_id, 0),
  NULLIF(TRIM(x.noted_by_name), ''),
  NULLIF(x.approved_by_id, 0),
  NULLIF(TRIM(x.approved_by_name), ''),
  x.turn_around_time,
  CASE WHEN x.turn_around_time IS NOT NULL THEN 'days' ELSE NULL END,
  COALESCE(x.archived, 0),
  CASE WHEN x.arch_dt IS NULL THEN NULL ELSE DATE_SUB(x.arch_dt, INTERVAL 8 HOUR) END,
  0
FROM tmp_rqst_requests x
LEFT JOIN users req_u ON req_u.legacy_employee_id = x.requested_by_id
LEFT JOIN teams req_team ON req_team.legacy_office_id = x.office_id
LEFT JOIN users ap_u ON ap_u.legacy_employee_id = NULLIF(x.approved_by_id, 0)
LEFT JOIN users pa_u ON pa_u.legacy_employee_id = COALESCE(NULLIF(x.personnel1_id,0), NULLIF(x.personnel2_id,0), NULLIF(x.personnel3_id,0), NULLIF(x.personnel4_id,0))
ON DUPLICATE KEY UPDATE
  ticket_number=VALUES(ticket_number), requester_id=VALUES(requester_id), requester_department_id=VALUES(requester_department_id),
  requester_name_snapshot=VALUES(requester_name_snapshot), subject=VALUES(subject), description=VALUES(description),
  status_id=VALUES(status_id), priority_id=VALUES(priority_id), category_id=VALUES(category_id),
  assigned_team_id=VALUES(assigned_team_id), primary_assignee_id=VALUES(primary_assignee_id),
  created_at_utc=VALUES(created_at_utc), updated_at_utc=VALUES(updated_at_utc), first_response_at_utc=VALUES(first_response_at_utc),
  resolved_at_utc=VALUES(resolved_at_utc), closed_at_utc=VALUES(closed_at_utc), canceled_at_utc=VALUES(canceled_at_utc),
  resolved_by_id=VALUES(resolved_by_id), closed_by_id=VALUES(closed_by_id), canceled_by_id=VALUES(canceled_by_id),
  cancel_reason=VALUES(cancel_reason), resolution_summary=VALUES(resolution_summary),
  satisfaction_rating=VALUES(satisfaction_rating), satisfaction_comment=VALUES(satisfaction_comment), satisfaction_rated_at_utc=VALUES(satisfaction_rated_at_utc),
  legacy_noted_by_id=VALUES(legacy_noted_by_id), legacy_noted_by_name=VALUES(legacy_noted_by_name),
  legacy_approved_by_id=VALUES(legacy_approved_by_id), legacy_approved_by_name=VALUES(legacy_approved_by_name),
  legacy_turnaround_value=VALUES(legacy_turnaround_value), legacy_turnaround_unit=VALUES(legacy_turnaround_unit),
  legacy_is_archived=VALUES(legacy_is_archived), legacy_archived_at_utc=VALUES(legacy_archived_at_utc);

DROP TEMPORARY TABLE IF EXISTS tmp_rqst_requests;

-- ---------------------------------------------------------------------------
-- 6) Ticket assignees from legacy personnel slots
-- ---------------------------------------------------------------------------
INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at_utc, unassigned_at_utc)
SELECT t.id, u.id, 1,
       COALESCE(t.first_response_at_utc, t.created_at_utc),
       NULL
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = NULLIF(r.personnel1_id, 0)
WHERE NULLIF(r.personnel1_id, 0) IS NOT NULL;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at_utc, unassigned_at_utc)
SELECT t.id, u.id, CASE WHEN NULLIF(r.personnel1_id,0) IS NULL THEN 1 ELSE 0 END,
       COALESCE(t.first_response_at_utc, t.created_at_utc),
       NULL
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = NULLIF(r.personnel2_id, 0)
WHERE NULLIF(r.personnel2_id, 0) IS NOT NULL;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at_utc, unassigned_at_utc)
SELECT t.id, u.id, CASE WHEN NULLIF(r.personnel1_id,0) IS NULL AND NULLIF(r.personnel2_id,0) IS NULL THEN 1 ELSE 0 END,
       COALESCE(t.first_response_at_utc, t.created_at_utc),
       NULL
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = NULLIF(r.personnel3_id, 0)
WHERE NULLIF(r.personnel3_id, 0) IS NOT NULL;

INSERT IGNORE INTO ticket_assignees (ticket_id, user_id, is_primary, assigned_at_utc, unassigned_at_utc)
SELECT t.id, u.id, CASE WHEN NULLIF(r.personnel1_id,0) IS NULL AND NULLIF(r.personnel2_id,0) IS NULL AND NULLIF(r.personnel3_id,0) IS NULL THEN 1 ELSE 0 END,
       COALESCE(t.first_response_at_utc, t.created_at_utc),
       NULL
FROM rqst.requests r
JOIN tickets t ON t.legacy_request_id = r.request_id
JOIN users u ON u.legacy_employee_id = NULLIF(r.personnel4_id, 0)
WHERE NULLIF(r.personnel4_id, 0) IS NOT NULL;

-- Agent role for users who were assigned as personnel
INSERT IGNORE INTO user_roles (user_id, role_id)
SELECT DISTINCT ta.user_id, 2
FROM ticket_assignees ta
JOIN users u ON u.id = ta.user_id
WHERE ta.unassigned_at_utc IS NULL;

-- Ensure primary assignee on ticket matches a current primary assignment where present
UPDATE tickets t
JOIN (
  SELECT ticket_id, MIN(user_id) AS user_id
  FROM ticket_assignees
  WHERE unassigned_at_utc IS NULL AND is_primary = 1
  GROUP BY ticket_id
) p ON p.ticket_id = t.id
SET t.primary_assignee_id = p.user_id
WHERE t.primary_assignee_id IS NULL OR t.primary_assignee_id <> p.user_id;

-- Deduplicate assignees (script may be re-run; table has no unique constraint on assignment rows)
DELETE a1
FROM ticket_assignees a1
INNER JOIN ticket_assignees a2
  ON a1.ticket_id = a2.ticket_id
 AND a1.user_id = a2.user_id
 AND (a1.assigned_at_utc <=> a2.assigned_at_utc)
 AND (a1.unassigned_at_utc <=> a2.unassigned_at_utc)
 AND a1.is_primary = a2.is_primary
 AND a1.id > a2.id;

-- ---------------------------------------------------------------------------
-- 7) Ticket number sequence (continue after MIS-26K012)
-- ---------------------------------------------------------------------------
INSERT INTO ticket_number_sequences (scope_key, last_value)
VALUES ('MIS-26K', 12)
ON DUPLICATE KEY UPDATE last_value = GREATEST(last_value, VALUES(last_value));

-- ---------------------------------------------------------------------------
-- 8) Validation summary
-- ---------------------------------------------------------------------------
SELECT 'teams' AS entity, COUNT(*) AS cnt FROM teams
UNION ALL SELECT 'users', COUNT(*) FROM users WHERE legacy_employee_id IS NOT NULL
UNION ALL SELECT 'tickets', COUNT(*) FROM tickets
UNION ALL SELECT 'ticket_assignees', COUNT(*) FROM ticket_assignees
UNION ALL SELECT 'statuses_seeded', COUNT(*) FROM ticket_statuses
UNION ALL SELECT 'priorities_seeded', COUNT(*) FROM ticket_priorities
UNION ALL SELECT 'roles_seeded', COUNT(*) FROM roles;

SELECT ts.code AS legacy_target_status_code, COUNT(*) AS tickets
FROM tickets t JOIN ticket_statuses ts ON ts.id = t.status_id
GROUP BY ts.code
ORDER BY tickets DESC;

COMMIT;

SET SESSION sql_mode = @OLD_SQL_MODE;
SET FOREIGN_KEY_CHECKS = @OLD_FOREIGN_KEY_CHECKS;
