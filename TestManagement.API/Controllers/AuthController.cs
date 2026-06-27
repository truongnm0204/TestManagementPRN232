using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.BAL.DTOs.Auth;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.LoginAsync(request);

        if (!result.Success || result.Data == null)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok("Đăng xuất thành công. Vui lòng xóa token ở phía client.");
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized("Không tìm thấy thông tin người dùng.");
        }

        var result = await _authService.GetCurrentUserAsync(userId.Value);

        if (!result.Success || result.Data == null)
        {
            return Unauthorized(result.Error);
        }

        return Ok(result.Data);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("Không tìm thấy thông tin người dùng.");

        var result = await _authService.UpdateProfileAsync(userId.Value, request);
        if (!result.Success || result.Data == null)
            return BadRequest(result.Error);

        return Ok(result.Data);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized("Không tìm thấy thông tin người dùng.");
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, request);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok("Đổi mật khẩu thành công.");
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
