namespace ServiceDesk.Application.Security;

/// <summary>
/// Permission claim values stored on roles (claim type: "permission") and evaluated by authorization policies.
/// Names align with docs/architecture.md.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Tickets
    {
        public const string Create = "tickets.create";
        public const string ViewTeam = "tickets.view.team";
        public const string ViewAll = "tickets.view.all";
        public const string InternalNotes = "tickets.internal_notes";
        public const string Workflow = "tickets.workflow";
        public const string AssignSelf = "tickets.assign.self";
        public const string Assign = "tickets.assign";
        public const string Priority = "tickets.priority";
        public const string Edit = "tickets.edit";
    }

    public static class Reports
    {
        public const string Team = "reports.team";
        public const string All = "reports.all";
    }

    public static class Admin
    {
        public const string UsersManage = "users.manage";
        public const string TeamsManage = "teams.manage";
        public const string CatalogManage = "catalog.manage";
        public const string SlaManage = "sla.manage";
        public const string RolesManage = "roles.manage";
        public const string SettingsManage = "settings.manage";
        public const string AuditView = "audit.view";
        public const string RecycleBinManage = "recyclebin.manage";
    }

    public static IReadOnlyList<string> All { get; } =
    [
        Tickets.Create, Tickets.ViewTeam, Tickets.ViewAll, Tickets.InternalNotes, Tickets.Workflow,
        Tickets.AssignSelf, Tickets.Assign, Tickets.Priority, Tickets.Edit,
        Reports.Team, Reports.All,
        Admin.UsersManage, Admin.TeamsManage, Admin.CatalogManage, Admin.SlaManage,
        Admin.RolesManage, Admin.SettingsManage, Admin.AuditView, Admin.RecycleBinManage
    ];

    public static IReadOnlyDictionary<string, string[]> RoleDefaults { get; } = new Dictionary<string, string[]>
    {
        ["Requester"] =
        [
            Tickets.Create,
            // tickets.view.team/.all handled by resource-based access to own/watched; workflow limited by business rules
            Tickets.Workflow,
            Tickets.Edit
        ],
        ["Agent"] =
        [
            Tickets.Create,
            Tickets.ViewTeam,
            Tickets.InternalNotes,
            Tickets.Workflow,
            Tickets.AssignSelf,
            Tickets.Edit
        ],
        ["TeamLead"] =
        [
            Tickets.Create,
            Tickets.ViewTeam,
            Tickets.InternalNotes,
            Tickets.Workflow,
            Tickets.AssignSelf,
            Tickets.Assign,
            Tickets.Priority,
            Tickets.Edit,
            Reports.Team,
            Admin.AuditView
        ],
        ["Admin"] = [.. All]
    };
}

public static class AppRoleNames
{
    public const string Requester = "Requester";
    public const string Agent = "Agent";
    public const string TeamLead = "TeamLead";
    public const string Admin = "Admin";

    public static readonly string[] All = [Requester, Agent, TeamLead, Admin];
}
