namespace TestManagement.BAL.DTOs.Notifications;

// Trả về cho client khi lấy danh sách / nhận realtime
public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Type { get; set; }
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
