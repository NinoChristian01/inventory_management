using System.ComponentModel.DataAnnotations;

namespace ServiceDesk.Web.ViewModels.Auth;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Enter your username.")]
    [Display(Name = "Username or employee number")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter your password.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Keep me signed in")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
