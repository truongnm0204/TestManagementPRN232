using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.ExamResults;

namespace TestManagement.Client.Services;

// Wrap ApiClient cho các endpoint của ExamResultsController (Teacher,Admin)
public class ExamResultService
{
    private readonly ApiClient _apiClient;

    public ExamResultService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // GET api/exam-results/assignments
    public Task<ApiResult<List<ExamAssignmentSummaryViewModel>>> GetAssignmentsAsync()
    {
        return _apiClient.GetAsync<List<ExamAssignmentSummaryViewModel>>("api/exam-results/assignments");
    }

    // GET api/exam-results/class?examId={}&classId={}
    public Task<ApiResult<ClassResultViewModel>> GetClassResultAsync(int examId, int classId)
    {
        return _apiClient.GetAsync<ClassResultViewModel>($"api/exam-results/class?examId={examId}&classId={classId}");
    }

    // GET api/exam-results/attempts/{attemptId}/history
    public Task<ApiResult<List<AnswerHistoryViewModel>>> GetAttemptHistoryAsync(int attemptId)
    {
        return _apiClient.GetAsync<List<AnswerHistoryViewModel>>($"api/exam-results/attempts/{attemptId}/history");
    }
}
