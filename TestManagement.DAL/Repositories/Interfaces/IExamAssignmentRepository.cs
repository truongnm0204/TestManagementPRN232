using TestManagement.DAL.Models;

namespace TestManagement.DAL.Repositories.Interfaces;

public interface IExamAssignmentRepository : IRepository<ExamAssignment>
{
    Task<List<ExamAssignment>> GetByExamIdAsync(int examId);
    Task<List<ExamAssignment>> GetAllWithDetailsAsync();
    Task<ExamAssignment?> GetDetailAsync(int id);
    Task<bool> ExistsAsync(int examId, int classId);
    /// <summary>Lấy assignment của tất cả lớp mà student đang thuộc</summary>
    Task<List<ExamAssignment>> GetByStudentAsync(int studentId);
}
