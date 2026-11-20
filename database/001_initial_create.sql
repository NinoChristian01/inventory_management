CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `app_settings` (
        `setting_key` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `setting_value` longtext CHARACTER SET utf8mb4 NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_app_settings` PRIMARY KEY (`setting_key`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `audit_logs` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `actor_id` bigint NULL,
        `actor_name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `activity_type` smallint NOT NULL,
        `table_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `record_id` bigint NULL,
        `description` varchar(500) CHARACTER SET utf8mb4 NULL,
        `old_values` json NULL,
        `new_values` json NULL,
        `created_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_audit_logs` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `business_hours` (
        `id` int NOT NULL AUTO_INCREMENT,
        `day_of_week` tinyint unsigned NOT NULL,
        `start_time` time(6) NOT NULL,
        `end_time` time(6) NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        CONSTRAINT `pk_business_hours` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `deleted_records` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `entity_name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `record_id` bigint NOT NULL,
        `display_name` varchar(255) CHARACTER SET utf8mb4 NULL,
        `snapshot` json NOT NULL,
        `deleted_by_id` bigint NULL,
        `deleted_by_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `deleted_at_utc` datetime(6) NOT NULL,
        `restored_at_utc` datetime(6) NULL,
        CONSTRAINT `pk_deleted_records` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `holidays` (
        `id` int NOT NULL AUTO_INCREMENT,
        `holiday_date` date NOT NULL,
        `name` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `is_recurring` tinyint(1) NOT NULL,
        CONSTRAINT `pk_holidays` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `roles` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `is_system` tinyint(1) NOT NULL DEFAULT FALSE,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        `name` varchar(256) CHARACTER SET utf8mb4 NULL,
        `normalized_name` varchar(256) CHARACTER SET utf8mb4 NULL,
        `concurrency_stamp` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `pk_roles` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_number_sequences` (
        `scope_key` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `last_value` int NOT NULL,
        CONSTRAINT `pk_ticket_number_sequences` PRIMARY KEY (`scope_key`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_priorities` (
        `id` smallint NOT NULL AUTO_INCREMENT,
        `code` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `color` varchar(20) CHARACTER SET utf8mb4 NULL,
        `sort_order` tinyint unsigned NOT NULL,
        CONSTRAINT `pk_ticket_priorities` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_statuses` (
        `id` smallint NOT NULL AUTO_INCREMENT,
        `code` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `name` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `is_open` tinyint(1) NOT NULL,
        `is_terminal` tinyint(1) NOT NULL,
        `pauses_sla` tinyint(1) NOT NULL,
        `sort_order` tinyint unsigned NOT NULL,
        CONSTRAINT `pk_ticket_statuses` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `role_claims` (
        `id` int NOT NULL AUTO_INCREMENT,
        `role_id` bigint NOT NULL,
        `claim_type` varchar(100) CHARACTER SET utf8mb4 NULL,
        `claim_value` varchar(200) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `pk_role_claims` PRIMARY KEY (`id`),
        CONSTRAINT `fk_role_claims_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `notifications` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `recipient_id` bigint NOT NULL,
        `ticket_id` bigint NULL,
        `type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `title` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `body` longtext CHARACTER SET utf8mb4 NULL,
        `link_url` varchar(500) CHARACTER SET utf8mb4 NULL,
        `is_read` tinyint(1) NOT NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `read_at_utc` datetime(6) NULL,
        CONSTRAINT `pk_notifications` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `sla_policies` (
        `id` int NOT NULL AUTO_INCREMENT,
        `priority_id` smallint NOT NULL,
        `category_id` int NULL,
        `response_target_minutes` int NOT NULL,
        `resolution_target_minutes` int NOT NULL,
        `use_business_hours` tinyint(1) NOT NULL,
        `warning_percent` tinyint unsigned NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_sla_policies` PRIMARY KEY (`id`),
        CONSTRAINT `fk_sla_policies_ticket_priorities_priority_id` FOREIGN KEY (`priority_id`) REFERENCES `ticket_priorities` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `team_members` (
        `team_id` int NOT NULL,
        `user_id` bigint NOT NULL,
        `is_lead` tinyint(1) NOT NULL DEFAULT FALSE,
        `joined_at_utc` datetime(6) NOT NULL,
        `left_at_utc` datetime(6) NULL,
        CONSTRAINT `pk_team_members` PRIMARY KEY (`team_id`, `user_id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `teams` (
        `id` int NOT NULL AUTO_INCREMENT,
        `code` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `name` varchar(250) CHARACTER SET utf8mb4 NOT NULL,
        `is_service_team` tinyint(1) NOT NULL,
        `is_active` tinyint(1) NOT NULL,
        `is_deleted` tinyint(1) NOT NULL,
        `manager_user_id` bigint NULL,
        `legacy_office_id` int NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_teams` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_categories` (
        `id` int NOT NULL AUTO_INCREMENT,
        `name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `parent_id` int NULL,
        `default_team_id` int NULL,
        `is_active` tinyint(1) NOT NULL,
        `is_deleted` tinyint(1) NOT NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_ticket_categories` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_categories_teams_default_team_id` FOREIGN KEY (`default_team_id`) REFERENCES `teams` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_ticket_categories_ticket_categories_parent_id` FOREIGN KEY (`parent_id`) REFERENCES `ticket_categories` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `users` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `employee_number` varchar(50) CHARACTER SET utf8mb4 NULL,
        `last_name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `first_name` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `middle_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `suffix` varchar(30) CHARACTER SET utf8mb4 NULL,
        `full_name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `department_id` int NULL,
        `is_active` tinyint(1) NOT NULL,
        `must_change_password` tinyint(1) NOT NULL,
        `legacy_employee_id` bigint NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        `user_name` varchar(256) CHARACTER SET utf8mb4 NULL,
        `normalized_user_name` varchar(256) CHARACTER SET utf8mb4 NULL,
        `email` varchar(256) CHARACTER SET utf8mb4 NULL,
        `normalized_email` varchar(256) CHARACTER SET utf8mb4 NULL,
        `email_confirmed` tinyint(1) NOT NULL,
        `password_hash` longtext CHARACTER SET utf8mb4 NULL,
        `security_stamp` longtext CHARACTER SET utf8mb4 NULL,
        `concurrency_stamp` longtext CHARACTER SET utf8mb4 NULL,
        `phone_number` longtext CHARACTER SET utf8mb4 NULL,
        `phone_number_confirmed` tinyint(1) NOT NULL,
        `two_factor_enabled` tinyint(1) NOT NULL,
        `lockout_end` datetime(6) NULL,
        `lockout_enabled` tinyint(1) NOT NULL,
        `access_failed_count` int NOT NULL,
        CONSTRAINT `pk_users` PRIMARY KEY (`id`),
        CONSTRAINT `fk_users_teams_department_id` FOREIGN KEY (`department_id`) REFERENCES `teams` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `tickets` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_number` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `requester_id` bigint NULL,
        `requester_department_id` int NULL,
        `requester_name_snapshot` varchar(300) CHARACTER SET utf8mb4 NULL,
        `subject` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `description` longtext CHARACTER SET utf8mb4 NULL,
        `status_id` smallint NOT NULL,
        `priority_id` smallint NOT NULL,
        `category_id` int NULL,
        `assigned_team_id` int NULL,
        `primary_assignee_id` bigint NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NOT NULL,
        `first_response_at_utc` datetime(6) NULL,
        `response_due_at_utc` datetime(6) NULL,
        `resolution_due_at_utc` datetime(6) NULL,
        `resolved_at_utc` datetime(6) NULL,
        `closed_at_utc` datetime(6) NULL,
        `canceled_at_utc` datetime(6) NULL,
        `last_reopened_at_utc` datetime(6) NULL,
        `resolved_by_id` bigint NULL,
        `closed_by_id` bigint NULL,
        `close_reason` varchar(500) CHARACTER SET utf8mb4 NULL,
        `canceled_by_id` bigint NULL,
        `cancel_reason` varchar(500) CHARACTER SET utf8mb4 NULL,
        `resolution_code` varchar(50) CHARACTER SET utf8mb4 NULL,
        `resolution_summary` longtext CHARACTER SET utf8mb4 NULL,
        `satisfaction_rating` tinyint unsigned NULL,
        `satisfaction_comment` longtext CHARACTER SET utf8mb4 NULL,
        `satisfaction_rated_at_utc` datetime(6) NULL,
        `legacy_request_id` bigint NULL,
        `legacy_noted_by_id` bigint NULL,
        `legacy_noted_by_name` varchar(200) CHARACTER SET utf8mb4 NULL,
        `legacy_approved_by_id` bigint NULL,
        `legacy_approved_by_name` varchar(200) CHARACTER SET utf8mb4 NULL,
        `legacy_turnaround_value` int NULL,
        `legacy_turnaround_unit` varchar(20) CHARACTER SET utf8mb4 NULL,
        `legacy_is_archived` tinyint(1) NOT NULL,
        `legacy_archived_at_utc` datetime(6) NULL,
        `version` int NOT NULL,
        CONSTRAINT `pk_tickets` PRIMARY KEY (`id`),
        CONSTRAINT `fk_tickets_teams_assigned_team_id` FOREIGN KEY (`assigned_team_id`) REFERENCES `teams` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_teams_requester_department_id` FOREIGN KEY (`requester_department_id`) REFERENCES `teams` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_ticket_categories_category_id` FOREIGN KEY (`category_id`) REFERENCES `ticket_categories` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_ticket_priorities_priority_id` FOREIGN KEY (`priority_id`) REFERENCES `ticket_priorities` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_tickets_ticket_statuses_status_id` FOREIGN KEY (`status_id`) REFERENCES `ticket_statuses` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_tickets_users_canceled_by_id` FOREIGN KEY (`canceled_by_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_users_closed_by_id` FOREIGN KEY (`closed_by_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_users_primary_assignee_id` FOREIGN KEY (`primary_assignee_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_users_requester_id` FOREIGN KEY (`requester_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_tickets_users_resolved_by_id` FOREIGN KEY (`resolved_by_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `user_claims` (
        `id` int NOT NULL AUTO_INCREMENT,
        `user_id` bigint NOT NULL,
        `claim_type` varchar(100) CHARACTER SET utf8mb4 NULL,
        `claim_value` varchar(200) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `pk_user_claims` PRIMARY KEY (`id`),
        CONSTRAINT `fk_user_claims_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `user_logins` (
        `login_provider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `provider_key` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `provider_display_name` longtext CHARACTER SET utf8mb4 NULL,
        `user_id` bigint NOT NULL,
        CONSTRAINT `pk_user_logins` PRIMARY KEY (`login_provider`, `provider_key`),
        CONSTRAINT `fk_user_logins_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `user_roles` (
        `user_id` bigint NOT NULL,
        `role_id` bigint NOT NULL,
        CONSTRAINT `pk_user_roles` PRIMARY KEY (`user_id`, `role_id`),
        CONSTRAINT `fk_user_roles_roles_role_id` FOREIGN KEY (`role_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_user_roles_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `user_tokens` (
        `user_id` bigint NOT NULL,
        `login_provider` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `value` longtext CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `pk_user_tokens` PRIMARY KEY (`user_id`, `login_provider`, `name`),
        CONSTRAINT `fk_user_tokens_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_activity_logs` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_id` bigint NOT NULL,
        `actor_id` bigint NULL,
        `actor_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `event_type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `field_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `old_value` longtext CHARACTER SET utf8mb4 NULL,
        `new_value` longtext CHARACTER SET utf8mb4 NULL,
        `metadata_json` json NULL,
        `created_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_ticket_activity_logs` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_activity_logs_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_ticket_activity_logs_users_actor_id` FOREIGN KEY (`actor_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_assignees` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_id` bigint NOT NULL,
        `user_id` bigint NOT NULL,
        `is_primary` tinyint(1) NOT NULL,
        `assigned_at_utc` datetime(6) NOT NULL,
        `unassigned_at_utc` datetime(6) NULL,
        CONSTRAINT `pk_ticket_assignees` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_assignees_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_ticket_assignees_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE RESTRICT
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_comments` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_id` bigint NOT NULL,
        `author_id` bigint NULL,
        `visibility` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `comment_type` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `body` longtext CHARACTER SET utf8mb4 NOT NULL,
        `created_at_utc` datetime(6) NOT NULL,
        `updated_at_utc` datetime(6) NULL,
        CONSTRAINT `pk_ticket_comments` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_comments_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_ticket_comments_users_author_id` FOREIGN KEY (`author_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_slas` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_id` bigint NOT NULL,
        `sla_policy_id` int NOT NULL,
        `metric` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `started_at_utc` datetime(6) NOT NULL,
        `due_at_utc` datetime(6) NOT NULL,
        `paused_at_utc` datetime(6) NULL,
        `completed_at_utc` datetime(6) NULL,
        `stage` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `is_breached` tinyint(1) NOT NULL,
        `breached_at_utc` datetime(6) NULL,
        `warning_sent` tinyint(1) NOT NULL,
        `breach_sent` tinyint(1) NOT NULL,
        `total_paused_minutes` int NOT NULL,
        CONSTRAINT `pk_ticket_slas` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_slas_sla_policies_sla_policy_id` FOREIGN KEY (`sla_policy_id`) REFERENCES `sla_policies` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `fk_ticket_slas_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_watchers` (
        `ticket_id` bigint NOT NULL,
        `user_id` bigint NOT NULL,
        `added_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_ticket_watchers` PRIMARY KEY (`ticket_id`, `user_id`),
        CONSTRAINT `fk_ticket_watchers_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_ticket_watchers_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE TABLE `ticket_attachments` (
        `id` bigint NOT NULL AUTO_INCREMENT,
        `ticket_id` bigint NOT NULL,
        `comment_id` bigint NULL,
        `uploaded_by_id` bigint NULL,
        `original_file_name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `stored_file_name` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `content_type` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
        `file_size_bytes` bigint NOT NULL,
        `sha256` varchar(64) CHARACTER SET utf8mb4 NOT NULL,
        `is_internal` tinyint(1) NOT NULL,
        `created_at_utc` datetime(6) NOT NULL,
        CONSTRAINT `pk_ticket_attachments` PRIMARY KEY (`id`),
        CONSTRAINT `fk_ticket_attachments_ticket_comments_comment_id` FOREIGN KEY (`comment_id`) REFERENCES `ticket_comments` (`id`) ON DELETE SET NULL,
        CONSTRAINT `fk_ticket_attachments_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE CASCADE,
        CONSTRAINT `fk_ticket_attachments_users_uploaded_by_id` FOREIGN KEY (`uploaded_by_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_audit_logs_created_at_utc` ON `audit_logs` (`created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_audit_logs_table_record` ON `audit_logs` (`table_name`, `record_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_business_hours_day_of_week` ON `business_hours` (`day_of_week`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_deleted_records_entity_restored` ON `deleted_records` (`entity_name`, `restored_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_holidays_holiday_date` ON `holidays` (`holiday_date`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_notifications_recipient_read_created` ON `notifications` (`recipient_id`, `is_read`, `created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_notifications_ticket_id` ON `notifications` (`ticket_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_role_claims_role_type_value` ON `role_claims` (`role_id`, `claim_type`, `claim_value`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `role_name_index` ON `roles` (`normalized_name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_sla_policies_category_id` ON `sla_policies` (`category_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_sla_policies_priority_category_active` ON `sla_policies` (`priority_id`, `category_id`, `is_active`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_team_members_user_id` ON `team_members` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_teams_is_service_team` ON `teams` (`is_service_team`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_teams_manager_user_id` ON `teams` (`manager_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_teams_code` ON `teams` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_teams_legacy_office_id` ON `teams` (`legacy_office_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_activity_logs_actor_id` ON `ticket_activity_logs` (`actor_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_activity_logs_event_type` ON `ticket_activity_logs` (`event_type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_activity_logs_ticket_created` ON `ticket_activity_logs` (`ticket_id`, `created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_assignees_ticket_primary_unassigned` ON `ticket_assignees` (`ticket_id`, `is_primary`, `unassigned_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_assignees_ticket_user_unassigned` ON `ticket_assignees` (`ticket_id`, `user_id`, `unassigned_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_assignees_user_id` ON `ticket_assignees` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_attachments_comment_id` ON `ticket_attachments` (`comment_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_attachments_ticket_id` ON `ticket_attachments` (`ticket_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_attachments_uploaded_by_id` ON `ticket_attachments` (`uploaded_by_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_categories_default_team_id` ON `ticket_categories` (`default_team_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_categories_is_active` ON `ticket_categories` (`is_active`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_categories_parent_id` ON `ticket_categories` (`parent_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_comments_author_id` ON `ticket_comments` (`author_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_comments_ticket_created` ON `ticket_comments` (`ticket_id`, `created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_ticket_priorities_code` ON `ticket_priorities` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_slas_due_at_utc` ON `ticket_slas` (`due_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_slas_sla_policy_id` ON `ticket_slas` (`sla_policy_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_slas_stage` ON `ticket_slas` (`stage`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_slas_ticket_metric` ON `ticket_slas` (`ticket_id`, `metric`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_ticket_statuses_code` ON `ticket_statuses` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_ticket_watchers_user_id` ON `ticket_watchers` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_assigned_team_id` ON `tickets` (`assigned_team_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_canceled_by_id` ON `tickets` (`canceled_by_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_category_id` ON `tickets` (`category_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_closed_by_id` ON `tickets` (`closed_by_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_created_at_utc` ON `tickets` (`created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_primary_assignee_id` ON `tickets` (`primary_assignee_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_priority_id` ON `tickets` (`priority_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_requester_department_id` ON `tickets` (`requester_department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_requester_id` ON `tickets` (`requester_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_resolved_by_id` ON `tickets` (`resolved_by_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_status_id` ON `tickets` (`status_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_tickets_status_priority_created` ON `tickets` (`status_id`, `priority_id`, `created_at_utc`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_tickets_legacy_request_id` ON `tickets` (`legacy_request_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_tickets_ticket_number` ON `tickets` (`ticket_number`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_user_claims_user_id` ON `user_claims` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_user_logins_user_id` ON `user_logins` (`user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_user_roles_role_id` ON `user_roles` (`role_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `email_index` ON `users` (`normalized_email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_users_department_id` ON `users` (`department_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE INDEX `ix_users_is_active` ON `users` (`is_active`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `user_name_index` ON `users` (`normalized_user_name`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_users_employee_number` ON `users` (`employee_number`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    CREATE UNIQUE INDEX `ux_users_legacy_employee_id` ON `users` (`legacy_employee_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `notifications` ADD CONSTRAINT `fk_notifications_tickets_ticket_id` FOREIGN KEY (`ticket_id`) REFERENCES `tickets` (`id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `notifications` ADD CONSTRAINT `fk_notifications_users_recipient_id` FOREIGN KEY (`recipient_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `sla_policies` ADD CONSTRAINT `fk_sla_policies_ticket_categories_category_id` FOREIGN KEY (`category_id`) REFERENCES `ticket_categories` (`id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `team_members` ADD CONSTRAINT `fk_team_members_teams_team_id` FOREIGN KEY (`team_id`) REFERENCES `teams` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `team_members` ADD CONSTRAINT `fk_team_members_users_user_id` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    ALTER TABLE `teams` ADD CONSTRAINT `fk_teams_users_manager_user_id` FOREIGN KEY (`manager_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20261120043819_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20261120043819_InitialCreate', '10.0.14');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

