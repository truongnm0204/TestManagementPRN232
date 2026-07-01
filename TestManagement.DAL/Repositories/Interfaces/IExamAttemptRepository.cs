using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface IExamAttemptRepository : IRepository<ExamAttempt>
{
    Task<List<ExamAttempt>> GetByStudentAsync(int studentId);
    Task<List<ExamAttempt>> GetByExamIdAsync(int examId);
    Task<ExamAttempt?> GetDetailAsync(int id);
    Task<ExamAttempt?> GetActiveAttemptAsync(int examId, int studentId);
    Task<int> CountAttemptsAsync(int examId, int studentId);
}
