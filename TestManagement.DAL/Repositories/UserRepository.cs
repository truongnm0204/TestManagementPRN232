using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await Context.Users
            .FirstOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
    }

    public async Task<User?> GetActiveByIdAsync(int id)
    {
        return await Context.Users
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        return await Context.Users.AnyAsync(x =>
            x.Email == email &&
            !x.IsDeleted &&
            (!excludeUserId.HasValue || x.Id != excludeUserId.Value));
    }

    public IQueryable<User> GetQueryable()
    {
        return Context.Users
            .AsNoTracking()
            .Where(x => !x.IsDeleted);
    }
}
