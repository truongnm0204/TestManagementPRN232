using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class NotificationRepository : Repository<Notification>, INotificationRepository
{
    public NotificationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<Notification>> GetByUserIdAsync(int userId, int take = 20)
    {
        return await Context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> CountUnreadAsync(int userId)
    {
        return await Context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<bool> MarkReadAsync(int id, int userId)
    {
        var notification = await Context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notification == null) return false;

        notification.IsRead = true;
        await Context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync(int userId)
    {
        var unread = await Context.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
        foreach (var n in unread) n.IsRead = true;
        await Context.SaveChangesAsync();
    }
}
