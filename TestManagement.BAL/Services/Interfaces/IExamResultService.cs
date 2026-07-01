using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamResults;

namespace TestManagement.BAL.Services.Interfaces;

public interface IExamResultService
{
    /// <summary>Danh sách các cặp (đề thi × lớp) đã giao — màn Index của Teacher</summary>
    Task<ServiceResult<List<ExamAssignmentSummaryDto>>> GetTeacherAssignmentsAsync();

    /// <summary>Bảng kết quả của cả lớp cho một đề thi</summary>
    Task<ServiceResult<ClassResultDto>> GetClassResultAsync(int examId, int classId);

    /// <summary>Lịch sử đổi đáp án của một attempt</summary>
    Task<ServiceResult<List<AnswerHistoryViewDto>>> GetAttemptHistoryAsync(int attemptId);
}
