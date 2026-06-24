using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class TopicRepository : Repository<Topic>, ITopicRepository
{
    public TopicRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Topic?> GetActiveByIdAsync(int id)
    {
        return await Context.Topics
            .Include(t => t.Subject)
            .FirstOrDefaultAsync(t => t.Id == id && t.Status == "Active");
    }

    public async Task<bool> NameExistsInSubjectAsync(string name, int subjectId, int? excludeTopicId = null)
    {
        return await Context.Topics.AnyAsync(t =>
            t.Name == name &&
            t.SubjectId == subjectId &&
            (!excludeTopicId.HasValue || t.Id != excludeTopicId.Value));
    }

    public IQueryable<Topic> GetQueryable()
    {
        return Context.Topics.AsNoTracking().Include(t => t.Subject);
    }

    public async Task<IEnumerable<Topic>> GetBySubjectIdAsync(int subjectId)
    {
        return await Context.Topics
            .Where(t => t.SubjectId == subjectId && t.Status == "Active")
            .OrderBy(t => t.DisplayOrder)
            .ToListAsync();
    }
   
}
