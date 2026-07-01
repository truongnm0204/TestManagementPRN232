using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAssignments;

namespace TestManagement.BAL.Services.Interfaces;

public interface IExamAssignmentService
{
    Task<ServiceResult<List<ExamAssignmentResponse>>> GetByExamIdAsync(int examId);
    Task<ServiceResult<ExamAssignmentResponse>> AssignAsync(int examId, AssignExamRequest request, int? currentUserId);
    Task<ServiceResult> RemoveAsync(int examId, int assignmentId);
}
