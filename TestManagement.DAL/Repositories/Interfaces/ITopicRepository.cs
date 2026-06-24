using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface ITopicRepository : IRepository<Topic>
{
    Task<Topic?> GetActiveByIdAsync(int id);
    Task<bool> NameExistsInSubjectAsync(string name, int subjectId, int? excludeTopicId = null);
    IQueryable<Topic> GetQueryable();
    Task<IEnumerable<Topic>> GetBySubjectIdAsync(int subjectId);
   
}
