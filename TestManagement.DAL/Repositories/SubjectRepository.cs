using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Subject?> GetActiveByIdAsync(int id)
    {
        return await Context.Subjects
            .Include(x => x.Questions.Where(q => !q.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeSubjectId = null)
    {
        return await Context.Subjects.AnyAsync(x =>
            x.Code == code &&
            !x.IsDeleted &&
            (!excludeSubjectId.HasValue || x.Id != excludeSubjectId.Value));
    }

    public async Task<bool> HasActiveQuestionsAsync(int subjectId)
    {
        return await Context.Questions.AnyAsync(x =>
            x.SubjectId == subjectId &&
            !x.IsDeleted &&
            x.Status == "Active");
    }

    public IQueryable<Subject> GetQueryable()
    {
        return Context.Subjects
            .AsNoTracking()
            .Where(x => !x.IsDeleted);
    }

    public async Task<Topic?> GetTopicByIdAsync(int value)
    {
        return await Context.Topics.FirstOrDefaultAsync(x => x.Id == value && x.Status == "Active");
    }

   
}
