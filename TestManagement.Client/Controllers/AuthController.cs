using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Auth;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

public class AuthController : Controller
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new LoginViewModel());
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

        var result = await _authService.LoginAsync(model);
        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Đăng nhập không thành công.");
            return View(model);
        }

        var user = result.Data.User;
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = result.Data.ExpiresAt
            });

        HttpContext.Session.SetString(SessionKeys.AccessToken, result.Data.AccessToken);
        TempData["Success"] = "Đăng nhập thành công.";
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        HttpContext.Session.Remove(SessionKeys.AccessToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Đã đăng xuất.";
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

        var result = await _authService.ChangePasswordAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Đổi mật khẩu không thành công.");
            return View(model);
        }

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
