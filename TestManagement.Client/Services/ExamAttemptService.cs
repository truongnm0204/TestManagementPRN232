using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.ExamAttempts;

namespace TestManagement.Client.Services;

// Lớp mỏng wrap ApiClient — map mỗi method tới 1 endpoint của ExamAttemptsController
public class ExamAttemptService
{
    private readonly ApiClient _apiClient;

    public ExamAttemptService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET api/exam-attempts/my-exams
    public Task<ApiResult<List<MyExamItemViewModel>>> GetMyExamsAsync()
    {
        return _apiClient.GetAsync<List<MyExamItemViewModel>>("api/exam-attempts/my-exams");
    }

    // POST api/exam-attempts — body { examId } → trả về đề + đáp án đã lưu
    public Task<ApiResult<TakeExamViewModel>> StartAsync(int examId)
    {
        var body = new { examId };
        return _apiClient.PostAsync<object, TakeExamViewModel>("api/exam-attempts", body);
    }

    // GET api/exam-attempts/{id} — lấy lại đề của attempt đang làm (resume khi refresh)
    public Task<ApiResult<TakeExamViewModel>> GetAttemptAsync(int attemptId)
    {
        return _apiClient.GetAsync<TakeExamViewModel>($"api/exam-attempts/{attemptId}");
    }

    // POST api/exam-attempts/{id}/answers — auto-save 1 câu
    public Task<ApiResult<string>> SaveAnswerAsync(int attemptId, SaveAnswerRequestModel model)
    {
        return _apiClient.PostAsync($"api/exam-attempts/{attemptId}/answers", model);
    }

    // POST api/exam-attempts/{id}/submit — nộp bài, nhận kết quả
    public Task<ApiResult<ExamResultViewModel>> SubmitAsync(int attemptId)
    {
        return _apiClient.PostEmptyAsync<ExamResultViewModel>($"api/exam-attempts/{attemptId}/submit");
    }

    // GET api/exam-attempts/{id}/result — xem lại kết quả
    public Task<ApiResult<ExamResultViewModel>> GetResultAsync(int attemptId)
    {
        return _apiClient.GetAsync<ExamResultViewModel>($"api/exam-attempts/{attemptId}/result");
    }
}
