using Microsoft.AspNetCore.SignalR;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Notifications;
using TestManagement.BAL.Hubs;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationService(INotificationRepository repository, IHubContext<NotificationHub> hubContext)
    {
        _repository = repository;
        _hubContext = hubContext;
    }

    public async Task CreateAndPushAsync(int userId, string title, string? content, string type, string? dataJson = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Content = content,
            Type = type,
            DataJson = dataJson,
            IsRead = false,
            CreatedAt = DateTime.Now
        };

        await _repository.AddAsync(notification);
        await _repository.SaveChangesAsync();

        // Push realtime tới group của user. Method client lắng nghe: "ReceiveNotification"
        await _hubContext.Clients.Group($"user-{userId}").SendAsync("ReceiveNotification", MapToDto(notification));
    }

    public async Task<ServiceResult<List<NotificationDto>>> GetMyAsync(int userId)
    {
        var items = await _repository.GetByUserIdAsync(userId);
        return ServiceResult<List<NotificationDto>>.Ok(items.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<int>> GetUnreadCountAsync(int userId)
    {
        var count = await _repository.CountUnreadAsync(userId);
        return ServiceResult<int>.Ok(count);
    }

    public async Task<ServiceResult> MarkReadAsync(int id, int userId)
    {
        var ok = await _repository.MarkReadAsync(id, userId);
        return ok ? ServiceResult.Ok() : ServiceResult.Fail("Không tìm thấy thông báo.");
    }

    public async Task<ServiceResult> MarkAllReadAsync(int userId)
    {
        await _repository.MarkAllReadAsync(userId);
        return ServiceResult.Ok();
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Content = n.Content,
        Type = n.Type,
        DataJson = n.DataJson,
        IsRead = n.IsRead,
        CreatedAt = n.CreatedAt
    };
}
