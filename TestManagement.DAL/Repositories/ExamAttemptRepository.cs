using Microsoft.EntityFrameworkCore;
using TestManagement.DAL.Data;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.DAL.Repositories;

public class ExamAttemptRepository : Repository<ExamAttempt>, IExamAttemptRepository
{
    public ExamAttemptRepository(ApplicationDbContext context) : base(context) { }

    public async Task<List<ExamAttempt>> GetByStudentAsync(int studentId)
    {
        return await Context.ExamAttempts
            .AsNoTracking()
            .Include(a => a.Exam).ThenInclude(e => e!.Subject)
            .Where(a => a.StudentId == studentId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync();
    }

    public async Task<List<ExamAttempt>> GetByExamIdAsync(int examId)
    {
        return await Context.ExamAttempts
            .AsNoTracking()
            .Include(a => a.Student)
            .Where(a => a.ExamId == examId)
            .OrderBy(a => a.Student!.FullName)
            .ToListAsync();
    }

    public async Task<ExamAttempt?> GetDetailAsync(int id)
    {
        return await Context.ExamAttempts
            .Include(a => a.Exam).ThenInclude(e => e!.Subject)
            .Include(a => a.Student)
            .Include(a => a.StudentAnswers)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ExamAttempt?> GetActiveAttemptAsync(int examId, int studentId)
    {
        return await Context.ExamAttempts
            .Include(a => a.StudentAnswers)
            .FirstOrDefaultAsync(a => a.ExamId == examId && a.StudentId == studentId && a.Status == "InProgress");
    }

    public async Task<int> CountAttemptsAsync(int examId, int studentId)
    {
        return await Context.ExamAttempts
            .CountAsync(a => a.ExamId == examId && a.StudentId == studentId && a.Status != "Abandoned");
    }
}
