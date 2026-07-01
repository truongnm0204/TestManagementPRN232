using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetByUserIdAsync(int userId, int take = 20);
    Task<int> CountUnreadAsync(int userId);
    Task<bool> MarkReadAsync(int id, int userId);
    Task MarkAllReadAsync(int userId);
}
