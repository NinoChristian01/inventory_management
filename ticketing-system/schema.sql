-- Industry-standard ticketing system schema (MySQL/MariaDB)
-- Designed as an improved, normalized replacement/evolution of the legacy `rqst` database.
-- Target: MariaDB 10.4+ / MySQL 8.0+ (InnoDB, utf8mb4)

CREATE DATABASE IF NOT EXISTS `ticketing_system`
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;
USE `ticketing_system`;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ---------------------------------------------------------------------------
-- Reference / lookup tables
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS roles (
  id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name            VARCHAR(50)  NOT NULL,
  description     VARCHAR(255) NULL,
  created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_roles_name (name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_statuses (
  id              TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code            VARCHAR(30)  NOT NULL,  -- machine key
  name            VARCHAR(50)  NOT NULL,  -- display label
  is_open         TINYINT(1)   NOT NULL DEFAULT 1,
  is_terminal     TINYINT(1)   NOT NULL DEFAULT 0,
  sort_order      TINYINT UNSIGNED NOT NULL DEFAULT 0,
  created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_ticket_statuses_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_priorities (
  id                      TINYINT UNSIGNED NOT NULL AUTO_INCREMENT,
  code                    VARCHAR(20)  NOT NULL,
  name                    VARCHAR(50)  NOT NULL,
  sort_order              TINYINT UNSIGNED NOT NULL DEFAULT 0,
  response_target_minutes  INT UNSIGNED NULL,
  resolution_target_minutes INT UNSIGNED NULL,
  color_hex               VARCHAR(7)   NULL,
  created_at              DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_ticket_priorities_code (code)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_categories (
  id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name            VARCHAR(100) NOT NULL,
  parent_id       INT UNSIGNED NULL,
  description     VARCHAR(500) NULL,
  default_team_id INT UNSIGNED NULL,
  is_active       TINYINT(1)   NOT NULL DEFAULT 1,
  created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_ticket_categories_parent (parent_id),
  KEY idx_ticket_categories_active (is_active),
  CONSTRAINT fk_ticket_categories_parent FOREIGN KEY (parent_id)
    REFERENCES ticket_categories (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS sla_policies (
  id                       INT UNSIGNED NOT NULL AUTO_INCREMENT,
  name                     VARCHAR(100) NOT NULL,
  description              VARCHAR(500) NULL,
  priority_id              TINYINT UNSIGNED NULL,
  category_id              INT UNSIGNED NULL,
  response_target_minutes   INT UNSIGNED NOT NULL,
  resolution_target_minutes INT UNSIGNED NOT NULL,
  business_hours_only      TINYINT(1)   NOT NULL DEFAULT 0,
  is_active                TINYINT(1)   NOT NULL DEFAULT 1,
  created_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at               DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_sla_priority (priority_id),
  KEY idx_sla_category (category_id),
  CONSTRAINT fk_sla_priority FOREIGN KEY (priority_id)
    REFERENCES ticket_priorities (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_sla_category FOREIGN KEY (category_id)
    REFERENCES ticket_categories (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------------
-- Organization / users
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS teams (
  id              INT UNSIGNED NOT NULL AUTO_INCREMENT,
  legacy_office_id INT NULL,                 -- traceability to rqst.offices.id
  name            VARCHAR(250) NOT NULL,
  code            VARCHAR(50)  NULL,
  email           VARCHAR(255) NULL,
  manager_user_id BIGINT UNSIGNED NULL,      -- FK added after users table
  is_active       TINYINT(1)   NOT NULL DEFAULT 1,
  is_service_team TINYINT(1)   NOT NULL DEFAULT 0,
  created_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_teams_legacy_office (legacy_office_id),
  UNIQUE KEY uk_teams_code (code),
  KEY idx_teams_active (is_active)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS users (
  id                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  legacy_employee_id BIGINT NULL,             -- traceability to rqst.employees.employee_id
  employee_number   VARCHAR(50)  NULL,
  username          VARCHAR(100) NULL,
  email             VARCHAR(255) NULL,
  password_hash     VARCHAR(255) NULL,       -- for local auth; nullable if SSO/external
  first_name        VARCHAR(100) NULL,
  middle_name       VARCHAR(100) NULL,
  last_name         VARCHAR(100) NULL,
  full_name         VARCHAR(255) NOT NULL,
  phone             VARCHAR(50)  NULL,
  team_id           INT UNSIGNED NULL,
  is_active         TINYINT(1)   NOT NULL DEFAULT 1,
  last_login_at     DATETIME     NULL,
  created_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at        DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  UNIQUE KEY uk_users_legacy_employee (legacy_employee_id),
  UNIQUE KEY uk_users_employee_number (employee_number),
  UNIQUE KEY uk_users_username (username),
  UNIQUE KEY uk_users_email (email),
  KEY idx_users_team (team_id),
  KEY idx_users_active (is_active),
  KEY idx_users_full_name (full_name),
  CONSTRAINT fk_users_team FOREIGN KEY (team_id)
    REFERENCES teams (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

ALTER TABLE teams
  ADD CONSTRAINT fk_teams_manager_user FOREIGN KEY (manager_user_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE;

CREATE TABLE IF NOT EXISTS user_roles (
  user_id     BIGINT UNSIGNED NOT NULL,
  role_id     INT UNSIGNED    NOT NULL,
  assigned_at DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (user_id, role_id),
  KEY idx_user_roles_role (role_id),
  CONSTRAINT fk_user_roles_user FOREIGN KEY (user_id)
    REFERENCES users (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_user_roles_role FOREIGN KEY (role_id)
    REFERENCES roles (id) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------------
-- Core ticketing
-- ---------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS tickets (
  id                    BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  legacy_request_id     BIGINT NULL,               -- traceability to rqst.requests.request_id
  ticket_number         VARCHAR(50)  NOT NULL,
  requester_id          BIGINT UNSIGNED NULL,
  requester_team_id     INT UNSIGNED NULL,         -- requester office/team at time of request
  requester_name        VARCHAR(255) NULL,         -- snapshot for history/reporting
  subject               VARCHAR(255) NOT NULL,
  description           TEXT         NULL,
  category_id           INT UNSIGNED NULL,
  priority_id           TINYINT UNSIGNED NOT NULL,
  status_id             TINYINT UNSIGNED NOT NULL,
  assigned_team_id      INT UNSIGNED NULL,
  primary_assignee_id   BIGINT UNSIGNED NULL,
  source                VARCHAR(30)  NOT NULL DEFAULT 'portal', -- portal,email,phone,chat,api,internal
  reported_via          VARCHAR(50)  NULL,
  noted_by_user_id      BIGINT UNSIGNED NULL,
  noted_by_name         VARCHAR(255) NULL,
  approved_by_user_id   BIGINT UNSIGNED NULL,
  approved_by_name      VARCHAR(255) NULL,
  sla_policy_id         INT UNSIGNED NULL,
  created_at            DATETIME     NOT NULL,
  updated_at            DATETIME     NOT NULL,
  first_response_at     DATETIME     NULL,
  due_at                DATETIME     NULL,
  resolved_at           DATETIME     NULL,
  closed_at             DATETIME     NULL,
  last_activity_at      DATETIME     NULL,
  resolution_code       VARCHAR(50)  NULL,
  resolution_summary    TEXT         NULL,
  turnaround_value      INT          NULL,
  turnaround_unit       VARCHAR(10)  NULL,        -- e.g. days, hours
  satisfaction_rating   TINYINT UNSIGNED NULL,    -- e.g. 1-5
  satisfaction_comment  TEXT         NULL,
  satisfaction_rated_at DATETIME     NULL,
  is_archived           TINYINT(1)   NOT NULL DEFAULT 0,
  archived_at           DATETIME     NULL,
  cancel_reason         VARCHAR(500) NULL,
  reopened_count        INT UNSIGNED NOT NULL DEFAULT 0,
  created_by            BIGINT UNSIGNED NULL,
  updated_by            BIGINT UNSIGNED NULL,
  PRIMARY KEY (id),
  UNIQUE KEY uk_tickets_ticket_number (ticket_number),
  UNIQUE KEY uk_tickets_legacy_request (legacy_request_id),
  KEY idx_tickets_requester (requester_id),
  KEY idx_tickets_status (status_id),
  KEY idx_tickets_priority (priority_id),
  KEY idx_tickets_assigned_team (assigned_team_id),
  KEY idx_tickets_primary_assignee (primary_assignee_id),
  KEY idx_tickets_category (category_id),
  KEY idx_tickets_created_at (created_at),
  KEY idx_tickets_updated_at (updated_at),
  KEY idx_tickets_due_at (due_at),
  KEY idx_tickets_archived (is_archived),
  KEY idx_tickets_status_priority_created (status_id, priority_id, created_at),
  KEY idx_tickets_team_status (assigned_team_id, status_id),
  FULLTEXT KEY ft_tickets_subject_description (subject, description),
  CONSTRAINT fk_tickets_requester FOREIGN KEY (requester_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_requester_team FOREIGN KEY (requester_team_id)
    REFERENCES teams (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_category FOREIGN KEY (category_id)
    REFERENCES ticket_categories (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_priority FOREIGN KEY (priority_id)
    REFERENCES ticket_priorities (id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_status FOREIGN KEY (status_id)
    REFERENCES ticket_statuses (id) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_assigned_team FOREIGN KEY (assigned_team_id)
    REFERENCES teams (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_primary_assignee FOREIGN KEY (primary_assignee_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_noted_by FOREIGN KEY (noted_by_user_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_approved_by FOREIGN KEY (approved_by_user_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_sla FOREIGN KEY (sla_policy_id)
    REFERENCES sla_policies (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_created_by FOREIGN KEY (created_by)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_tickets_updated_by FOREIGN KEY (updated_by)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_assignees (
  ticket_id    BIGINT UNSIGNED NOT NULL,
  user_id      BIGINT UNSIGNED NOT NULL,
  is_primary   TINYINT(1)      NOT NULL DEFAULT 0,
  assigned_at  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  assigned_by  BIGINT UNSIGNED NULL,
  unassigned_at DATETIME       NULL,
  PRIMARY KEY (ticket_id, user_id),
  KEY idx_ticket_assignees_user (user_id),
  KEY idx_ticket_assignees_primary (ticket_id, is_primary),
  CONSTRAINT fk_ticket_assignees_ticket FOREIGN KEY (ticket_id)
    REFERENCES tickets (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_assignees_user FOREIGN KEY (user_id)
    REFERENCES users (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_assignees_assigned_by FOREIGN KEY (assigned_by)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_watchers (
  ticket_id   BIGINT UNSIGNED NOT NULL,
  user_id     BIGINT UNSIGNED NOT NULL,
  added_at    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  added_by    BIGINT UNSIGNED NULL,
  PRIMARY KEY (ticket_id, user_id),
  KEY idx_ticket_watchers_user (user_id),
  CONSTRAINT fk_ticket_watchers_ticket FOREIGN KEY (ticket_id)
    REFERENCES tickets (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_watchers_user FOREIGN KEY (user_id)
    REFERENCES users (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_watchers_added_by FOREIGN KEY (added_by)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_comments (
  id            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  ticket_id     BIGINT UNSIGNED NOT NULL,
  author_id     BIGINT UNSIGNED NULL,
  author_name   VARCHAR(255)    NULL,
  body          TEXT            NOT NULL,
  is_internal   TINYINT(1)      NOT NULL DEFAULT 0, -- internal note vs public reply
  comment_type  VARCHAR(20)     NOT NULL DEFAULT 'comment', -- comment, system, resolution, feedback
  created_at    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at    DATETIME        NULL,
  PRIMARY KEY (id),
  KEY idx_ticket_comments_ticket_created (ticket_id, created_at),
  KEY idx_ticket_comments_author (author_id),
  CONSTRAINT fk_ticket_comments_ticket FOREIGN KEY (ticket_id)
    REFERENCES tickets (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_comments_author FOREIGN KEY (author_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_activity_logs (
  id          BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  ticket_id   BIGINT UNSIGNED NOT NULL,
  actor_id    BIGINT UNSIGNED NULL,
  actor_name  VARCHAR(255)    NULL,
  event_type  VARCHAR(50)     NOT NULL, -- created,status_changed,assigned,commented,updated,closed,...
  field_name  VARCHAR(100)    NULL,
  old_value   TEXT            NULL,
  new_value   TEXT            NULL,
  metadata    JSON            NULL,
  created_at  DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_ticket_activity_ticket_created (ticket_id, created_at),
  KEY idx_ticket_activity_actor (actor_id),
  KEY idx_ticket_activity_event (event_type),
  CONSTRAINT fk_ticket_activity_ticket FOREIGN KEY (ticket_id)
    REFERENCES tickets (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_activity_actor FOREIGN KEY (actor_id)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS ticket_attachments (
  id             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  ticket_id      BIGINT UNSIGNED NOT NULL,
  comment_id     BIGINT UNSIGNED NULL,
  uploaded_by    BIGINT UNSIGNED NULL,
  original_name  VARCHAR(255)    NOT NULL,
  stored_name    VARCHAR(255)    NOT NULL,
  storage_path   VARCHAR(1000)   NOT NULL,
  mime_type      VARCHAR(150)    NULL,
  file_size      BIGINT UNSIGNED NULL,
  checksum_sha256 CHAR(64)       NULL,
  created_at     DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (id),
  KEY idx_ticket_attachments_ticket (ticket_id),
  KEY idx_ticket_attachments_comment (comment_id),
  CONSTRAINT fk_ticket_attachments_ticket FOREIGN KEY (ticket_id)
    REFERENCES tickets (id) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_attachments_comment FOREIGN KEY (comment_id)
    REFERENCES ticket_comments (id) ON DELETE SET NULL ON UPDATE CASCADE,
  CONSTRAINT fk_ticket_attachments_user FOREIGN KEY (uploaded_by)
    REFERENCES users (id) ON DELETE SET NULL ON UPDATE CASCADE
) ENGINE=InnoDB;

-- ---------------------------------------------------------------------------
-- Seed data (industry-standard defaults)
-- ---------------------------------------------------------------------------

INSERT INTO roles (name, description) VALUES
  ('requester',  'Can create and track own tickets, reply, and confirm resolution'),
  ('agent',      'Can triage, respond to, update, and resolve tickets within scope'),
  ('team_lead',  'Can manage team queues, assignments, escalations, and priorities'),
  ('admin',      'Can configure system settings, users, roles, workflows, and SLAs')
ON DUPLICATE KEY UPDATE description = VALUES(description);

INSERT INTO ticket_statuses (code, name, is_open, is_terminal, sort_order) VALUES
  ('new',               'New',                1, 0, 10),
  ('open',              'Open',               1, 0, 20),
  ('in_progress',       'In Progress',        1, 0, 30),
  ('pending_requester', 'Pending Requester',  1, 0, 40),
  ('pending_third_party','Pending Third Party',1, 0, 50),
  ('on_hold',           'On Hold',            1, 0, 60),
  ('resolved',          'Resolved',           1, 0, 70),
  ('closed',            'Closed',             0, 1, 80),
  ('canceled',          'Canceled',           0, 1, 90),
  ('reopened',          'Reopened',           1, 0, 25)
ON DUPLICATE KEY UPDATE name = VALUES(name), is_open = VALUES(is_open), is_terminal = VALUES(is_terminal), sort_order = VALUES(sort_order);

INSERT INTO ticket_priorities (code, name, sort_order, response_target_minutes, resolution_target_minutes, color_hex) VALUES
  ('low',      'Low',      10, 1440, 7200, '#2E7D32'), -- 1d / 5d
  ('medium',   'Medium',   20,  480, 2880, '#1976D2'), -- 8h / 2d
  ('high',     'High',     30,  120, 1440, '#F57C00'), -- 2h / 1d
  ('critical', 'Critical', 40,   30,  480, '#C62828')  -- 30m / 8h
ON DUPLICATE KEY UPDATE name = VALUES(name), sort_order = VALUES(sort_order),
  response_target_minutes = VALUES(response_target_minutes),
  resolution_target_minutes = VALUES(resolution_target_minutes),
  color_hex = VALUES(color_hex);

INSERT INTO ticket_categories (name, description, is_active) VALUES
  ('General Service Request', 'Default category for migrated/general requests', 1)
ON DUPLICATE KEY UPDATE description = VALUES(description);

INSERT INTO sla_policies (name, description, priority_id, response_target_minutes, resolution_target_minutes, is_active)
SELECT CONCAT('Default - ', p.name), CONCAT('Default SLA for ', p.name, ' priority tickets'), p.id,
       p.response_target_minutes, p.resolution_target_minutes, 1
FROM ticket_priorities p
WHERE NOT EXISTS (SELECT 1 FROM sla_policies s WHERE s.name = CONCAT('Default - ', p.name));

-- Optional convenience view
CREATE OR REPLACE VIEW vw_ticket_queue AS
SELECT
  t.id,
  t.ticket_number,
  t.subject,
  ts.code AS status_code,
  ts.name AS status_name,
  tp.code AS priority_code,
  tp.name AS priority_name,
  req.full_name AS requester_name,
  rt.name AS requester_team_name,
  at.name AS assigned_team_name,
  aa.full_name AS primary_assignee_name,
  t.created_at,
  t.updated_at,
  t.due_at,
  t.resolved_at,
  t.closed_at,
  t.is_archived
FROM tickets t
JOIN ticket_statuses ts ON ts.id = t.status_id
JOIN ticket_priorities tp ON tp.id = t.priority_id
LEFT JOIN users req ON req.id = t.requester_id
LEFT JOIN teams rt ON rt.id = t.requester_team_id
LEFT JOIN teams at ON at.id = t.assigned_team_id
LEFT JOIN users aa ON aa.id = t.primary_assignee_id;

SET FOREIGN_KEY_CHECKS = 1;
