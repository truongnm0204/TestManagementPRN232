using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Notifications;

namespace TestManagement.BAL.Services.Interfaces;

public interface INotificationService
{
    /// <summary>Lưu notification vào DB và push realtime tới user qua SignalR</summary>
    Task CreateAndPushAsync(int userId, string title, string? content, string type, string? dataJson = null);

    Task<ServiceResult<List<NotificationDto>>> GetMyAsync(int userId);
    Task<ServiceResult<int>> GetUnreadCountAsync(int userId);
    Task<ServiceResult> MarkReadAsync(int id, int userId);
    Task<ServiceResult> MarkAllReadAsync(int userId);
}
