using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetActiveByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    IQueryable<User> GetQueryable();
}
