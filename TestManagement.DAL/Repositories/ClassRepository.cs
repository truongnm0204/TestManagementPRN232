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
            .Include(c => c.TeacherClasses)
                .ThenInclude(tc => tc.Teacher)
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

    public async Task<bool> IsTeacherInClassAsync(int teacherId, int classId)
    {
        return await Context.TeacherClasses.AnyAsync(tc =>
            tc.TeacherId == teacherId && tc.ClassId == classId);
    }

    public IQueryable<Class> GetQueryableForTeacher(int teacherId)
    {
        return Context.Classes
            .AsNoTracking()
            .Where(c => c.TeacherClasses.Any(tc => tc.TeacherId == teacherId));
    }
}
