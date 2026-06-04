using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface IQuestionRepository : IRepository<Question>
{
    Task<Question?> GetDetailAsync(int id);
    IQueryable<Question> GetQueryable();
    Task ReplaceOptionsAsync(Question question, IEnumerable<QuestionOption> options);
}
