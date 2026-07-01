using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAttempts;

namespace TestManagement.BAL.Services.Interfaces;

public interface IExamAttemptService
{
    /// <summary>Student xem danh sách đề thi được giao cho lớp mình</summary>
    Task<ServiceResult<List<MyExamResponse>>> GetMyExamsAsync(int studentId);

    /// <summary>Bắt đầu hoặc resume attempt</summary>
    Task<ServiceResult<StartAttemptResponse>> StartAsync(int examId, int studentId);

    /// <summary>Lấy lại đề của một attempt đang InProgress theo attemptId (dùng khi refresh trang làm bài)</summary>
    Task<ServiceResult<StartAttemptResponse>> GetAttemptAsync(int attemptId, int studentId);

    /// <summary>Lưu đáp án một câu (auto-save)</summary>
    Task<ServiceResult> SaveAnswerAsync(int attemptId, SaveAnswerRequest request, int studentId);

    /// <summary>Nộp bài — tính điểm tự động</summary>
    Task<ServiceResult<SubmitAttemptResponse>> SubmitAsync(int attemptId, int studentId);

    /// <summary>Xem kết quả attempt đã nộp</summary>
    Task<ServiceResult<SubmitAttemptResponse>> GetResultAsync(int attemptId, int studentId);
}
