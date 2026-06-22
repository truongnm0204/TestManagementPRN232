using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetActiveByIdAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeSubjectId = null);
    Task<bool> HasActiveQuestionsAsync(int subjectId);
    IQueryable<Subject> GetQueryable();
    Task<Topic?> GetTopicByIdAsync(int value);
}
