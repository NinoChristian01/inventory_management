namespace ServiceDesk.Web.ViewModels.Users;

public sealed class UserListItemViewModel
{
    public long Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public bool MustChangePassword { get; set; }
    public string Roles { get; set; } = string.Empty;
}
