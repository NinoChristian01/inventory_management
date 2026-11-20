using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Domain.Entities;
using ServiceDesk.Web.Helpers;
using ServiceDesk.Web.ViewModels.Auth;

namespace ServiceDesk.Web.Controllers;

[Route("account")]
public sealed class AccountController(
    SignInManager<User> signInManager,
    UserManager<User> userManager) : Controller
{
    [AllowAnonymous]
    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userName = model.UserName.Trim();
        var user = await userManager.FindByNameAsync(userName);
        if (user is null)
        {
            // Try by employee number / alternate lookup
            user = await userManager.Users.FirstOrDefaultAsync(u => u.EmployeeNumber == userName);
        }

        if (user is null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);
        if (result.Succeeded)
        {
            if (user.MustChangePassword)
            {
                TempData.ToastInfo("Please change your password to continue.");
                return RedirectToAction(nameof(ChangePassword));
            }

            TempData.ToastSuccess("Signed in successfully.");
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later or contact the MIS office.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password.");
        return View(model);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        TempData.ToastInfo("You have been signed out.");
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("change-password")]
    public IActionResult ChangePassword()
    {
        var forced = string.Equals(User.FindFirst("must_change_password")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        return View(new ChangePasswordViewModel { IsForced = forced });
    }

    [HttpPost("change-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        model.IsForced = string.Equals(User.FindFirst("must_change_password")?.Value, "true", StringComparison.OrdinalIgnoreCase);
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        if (user.MustChangePassword)
        {
            user.MustChangePassword = false;
            user.UpdatedAtUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
        }

        await userManager.UpdateSecurityStampAsync(user);
        await signInManager.RefreshSignInAsync(user);
        TempData.ToastSuccess("Your password has been updated.");
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet("access-denied")]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
