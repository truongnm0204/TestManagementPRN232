using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // GET api/notifications — danh sách thông báo của tôi
    [HttpGet]
    public async Task<IActionResult> GetMy()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.GetMyAsync(userId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // GET api/notifications/unread-count — số thông báo chưa đọc
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.GetUnreadCountAsync(userId.Value);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(new { count = result.Data });
    }

    // POST api/notifications/{id}/read — đánh dấu đã đọc
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.MarkReadAsync(id, userId.Value);
        if (!result.Success) return BadRequest(result.Error);
        return Ok();
    }

    // POST api/notifications/read-all — đánh dấu tất cả đã đọc
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _notificationService.MarkAllReadAsync(userId.Value);
        if (!result.Success) return BadRequest(result.Error);
        return Ok();
    }

    private int? GetCurrentUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}
