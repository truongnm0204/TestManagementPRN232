using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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

        await SignInFromApiResponse(result.Data);
        TempData["Success"] = "Đăng nhập thành công.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.RegisterAsync(model);
        if (!result.Success || result.Data == null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Đăng ký không thành công.");
            return View(model);
        }

        await SignInFromApiResponse(result.Data);
        TempData["Success"] = "Đăng ký thành công. Chào mừng bạn!";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult GoogleLogin(string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(GoogleSignedIn), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleSignedIn(string? returnUrl = null)
    {
        var authResult = await HttpContext.AuthenticateAsync("ExternalCookie");
        if (!authResult.Succeeded)
        {
            TempData["Error"] = "Đăng nhập Google thất bại. Vui lòng thử lại.";
            return RedirectToAction(nameof(Login));
        }

        var email = authResult.Principal?.FindFirstValue(ClaimTypes.Email);
        var fullName = authResult.Principal?.FindFirstValue(ClaimTypes.Name);

        await HttpContext.SignOutAsync("ExternalCookie");

        if (string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Không lấy được thông tin email từ Google.";
            return RedirectToAction(nameof(Login));
        }

        var result = await _authService.GoogleLoginAsync(email, fullName ?? email);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Đăng nhập Google thất bại.";
            return RedirectToAction(nameof(Login));
        }

        await SignInFromApiResponse(result.Data);
        TempData["Success"] = "Đăng nhập Google thành công.";
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
    public async Task<IActionResult> Profile()
    {
        var result = await _authService.GetCurrentUserAsync();
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể tải thông tin hồ sơ.";
            return RedirectToAction("Index", "Home");
        }

        return View(result.Data);
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
    [Authorize]
    public async Task<IActionResult> EditProfile()
    {
        var result = await _authService.GetCurrentUserAsync();
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể tải thông tin hồ sơ.";
            return RedirectToAction(nameof(Profile));
        }

        var model = new UpdateProfileViewModel
        {
            FullName = result.Data.FullName,
            PhoneNumber = result.Data.PhoneNumber
        };
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(UpdateProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.UpdateProfileAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Cập nhật thất bại.");
            return View(model);
        }

        TempData["Success"] = "Cập nhật hồ sơ thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePasswordFromProfile(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var profileResult = await _authService.GetCurrentUserAsync();
            TempData["ChangePasswordError"] = string.Join(" ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            return profileResult.Success && profileResult.Data != null
                ? View("Profile", profileResult.Data)
                : RedirectToAction(nameof(Profile));
        }

        var result = await _authService.ChangePasswordAsync(model);
        if (!result.Success)
        {
            TempData["ChangePasswordError"] = result.Error ?? "Đổi mật khẩu không thành công.";
            return RedirectToAction(nameof(Profile));
        }

        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInFromApiResponse(LoginResponseViewModel data)
    {
        var user = data.User;
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
            new AuthenticationProperties { IsPersistent = false, ExpiresUtc = data.ExpiresAt });
        HttpContext.Session.SetString(SessionKeys.AccessToken, data.AccessToken);
    }
}
