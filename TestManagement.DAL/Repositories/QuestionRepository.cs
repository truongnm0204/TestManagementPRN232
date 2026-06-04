using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class QuestionRepository : Repository<Question>, IQuestionRepository
{
    public QuestionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Question?> GetDetailAsync(int id)
    {
        return await Context.Questions
            .Include(x => x.Subject)
            .Include(x => x.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
    }

    public IQueryable<Question> GetQueryable()
    {
        return Context.Questions
            .AsNoTracking()
            .Include(x => x.Subject)
            .Where(x => !x.IsDeleted);
    }

    public async Task ReplaceOptionsAsync(Question question, IEnumerable<QuestionOption> options)
    {
        var currentOptions = await Context.QuestionOptions
            .Where(x => x.QuestionId == question.Id)
            .ToListAsync();

        Context.QuestionOptions.RemoveRange(currentOptions);
        await Context.QuestionOptions.AddRangeAsync(options);
    }
}
