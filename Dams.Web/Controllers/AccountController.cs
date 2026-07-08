using System.Security.Claims;
using Dams.Web.Data;
using Dams.Web.Models;
using Dams.Web.Services;
using Dams.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dams.Web.Controllers;

public class AccountController(DamsDbContext context, IPasswordService passwordService) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
        }

        if (model.DateOfBirth is not null && model.DateOfBirth.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.DateOfBirth), "Date of birth cannot be in the future.");
        }

        if (await context.Users.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email already exists.");
        }

        if (await context.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), "Phone number already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new User
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim().ToLowerInvariant(),
            PhoneNumber = model.PhoneNumber.Trim(),
            Gender = model.Gender,
            Role = AppRoles.Patient,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordService.HashPassword(user, model.Password);

        var patient = new Patient
        {
            User = user,
            DateOfBirth = model.DateOfBirth!.Value.Date
        };

        context.Patients.Add(patient);
        await context.SaveChangesAsync();
        await SignInAsync(user);

        TempData["SuccessMessage"] = "Registration completed successfully.";
        return RedirectToAction("Index", "Patient");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await context.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail);

        if (user is null || !passwordService.VerifyPassword(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "This account is disabled.");
            return View(model);
        }

        await SignInAsync(user);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRoleDashboard(user.Role);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId))
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        var user = await context.Users.FindAsync(parsedUserId);
        if (user is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        if (!passwordService.VerifyPassword(user, model.CurrentPassword))
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
            return View(model);
        }

        user.PasswordHash = passwordService.HashPassword(user, model.NewPassword);
        await context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Password changed successfully.";

        return RedirectToRoleDashboard(user.Role);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                AllowRefresh = true
            });
    }

    private IActionResult RedirectToRoleDashboard(string? role)
    {
        return role switch
        {
            AppRoles.Patient => RedirectToAction("Index", "Patient"),
            AppRoles.Doctor => RedirectToAction("Index", "Doctor"),
            AppRoles.Admin => RedirectToAction("Index", "Admin"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}
