using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface IClassRepository : IRepository<Class>
{
    Task<Class?> GetByIdWithStudentsAsync(int id);
    Task<bool> ClassCodeExistsAsync(string classCode, int? excludeClassId = null);
    IQueryable<Class> GetQueryable();
    Task<bool> IsStudentInClassAsync(int studentId, int classId);
}
