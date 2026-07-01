using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class ExamAssignmentRepository : Repository<ExamAssignment>, IExamAssignmentRepository
{
    public ExamAssignmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<List<ExamAssignment>> GetByExamIdAsync(int examId)
    {
        return await Context.ExamAssignments
            .AsNoTracking()
            .Include(x => x.Exam)
            .Include(x => x.Class)
            .Include(x => x.Assigner)
            .Where(x => x.ExamId == examId)
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<List<ExamAssignment>> GetAllWithDetailsAsync()
    {
        return await Context.ExamAssignments
            .AsNoTracking()
            .Include(x => x.Exam).ThenInclude(e => e!.Subject)
            .Include(x => x.Class)
            .Where(x => x.Exam!.IsPublished && !x.Exam.IsDeleted)
            .OrderByDescending(x => x.AssignedAt)
            .ToListAsync();
    }

    public async Task<ExamAssignment?> GetDetailAsync(int id)
    {
        return await Context.ExamAssignments
            .Include(x => x.Exam)
            .Include(x => x.Class)
            .Include(x => x.Assigner)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsAsync(int examId, int classId)
    {
        return await Context.ExamAssignments.AnyAsync(x => x.ExamId == examId && x.ClassId == classId);
    }

    public async Task<List<ExamAssignment>> GetByStudentAsync(int studentId)
    {
        // Lấy danh sách classId mà student đang thuộc (Status = Active)
        var classIds = await Context.StudentClasses
            .Where(sc => sc.StudentId == studentId && sc.Status == "Active")
            .Select(sc => sc.ClassId)
            .ToListAsync();

        if (!classIds.Any()) return new List<ExamAssignment>();

        return await Context.ExamAssignments
            .AsNoTracking()
            .Include(a => a.Exam).ThenInclude(e => e!.Subject)
            .Include(a => a.Class)
            .Where(a => classIds.Contains(a.ClassId))
            .ToListAsync();
    }
}
