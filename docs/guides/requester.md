# Requester Guide

Requesters use ServiceDesk to submit support requests, follow progress, reply to updates, and confirm outcomes for tickets they are allowed to access.

## Getting started
1. **Sign in** at `/account/login` with your username and password.
2. If your account was newly activated or reset, you may be redirected to **Change password** and must set a new password before continuing.
3. Use the **Dashboard** (`/`) for a quick view of open/related tickets and recent activity.

## What you can do

### Create a ticket
- Go to **Tickets → New ticket** (`/tickets/create`).
- Provide:
  - **Subject** and **Description** (required)
  - **Priority**, **Category**, and **Team** (as available)
- Submit to create the ticket and receive a ticket number.

> Note: Critical priority may be restricted based on your permissions/role.

### View and track tickets
- Go to **Tickets** (`/tickets`) to search/filter and open tickets you have access to (typically your own/request-related tickets, depending on system rules).
- Open a ticket to view:
  - Status, priority, category, team/assignee, requester/department
  - Key timestamps (created/updated, due dates if assigned, response/resolution/closure times when available)
  - Description, conversation/comments, attachments, activity history, and watchers

### Communicate on a ticket
- On the ticket details page, post a **reply** in the conversation.
- You can attach files to a reply (if enabled in the form; uploads are subject to size/count limits).
- Replies may trigger workflow/status behavior in some cases (for example, responding when additional information was requested), depending on ticket state and rules.

### Watch a ticket
- Use **Watch/Unwatch** on the ticket details page to follow updates.
- Watchers can receive in-app notifications for relevant ticket events.

### Notifications
- Open **Notifications** (`/notifications`) from the sidebar to review updates related to your tickets.
- Mark individual notifications as read or mark all as read.

### Account actions
- **Change password:** `/account/change-password`
- **Sign out:** use the sidebar sign-out action.

## Tips
- Include clear details in the description (what happened, when, where/system affected, and any steps already tried).
- Add follow-up information as a reply so the full history stays on the ticket.
- Check **Notifications** for updates from agents/leads/admins.

## Current limitations (may vary by deployment)
- Some self-service actions and full workflow options may depend on role permissions and current implementation.
- Reports/exports and advanced admin/reference-data features are not requester functions.