using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class ClassRepository : Repository<Class>, IClassRepository
{
    public ClassRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Class?> GetByIdWithStudentsAsync(int id)
    {
        return await Context.Classes
            .Include(c => c.StudentClasses)
                .ThenInclude(sc => sc.Student)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ClassCodeExistsAsync(string classCode, int? excludeClassId = null)
    {
        return await Context.Classes.AnyAsync(c =>
            c.ClassCode == classCode &&
            (!excludeClassId.HasValue || c.Id != excludeClassId.Value));
    }

    public IQueryable<Class> GetQueryable()
    {
        return Context.Classes.AsNoTracking();
    }

    public async Task<bool> IsStudentInClassAsync(int studentId, int classId)
    {
        return await Context.StudentClasses.AnyAsync(sc =>
            sc.StudentId == studentId &&
            sc.ClassId == classId &&
            sc.Status == "Active");
    }

    public async Task<StudentClass?> GetStudentClassAsync(int studentId, int classId)
    {
        return await Context.StudentClasses.FirstOrDefaultAsync(sc =>
            sc.StudentId == studentId &&
            sc.ClassId == classId);
    }
}
