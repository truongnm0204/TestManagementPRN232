using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.ExamAssignments;
using TestManagement.Client.Models.Exams;

namespace TestManagement.Client.Services;

public class ExamService
{
    private readonly ApiClient _apiClient;

    public ExamService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<ExamItemViewModel>>> GetListAsync(
        string? keyword,
        int? subjectId,
        string? status,
        bool? isPublished,
        int page,
        int pageSize)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Contains("Title", keyword));
        }

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            filters.Add($"SubjectId eq {subjectId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        }

        if (isPublished.HasValue)
        {
            filters.Add($"IsPublished eq {isPublished.Value.ToString().ToLowerInvariant()}");
        }

        var endpoint = ODataQueryBuilder.Build("api/exams", ODataQueryBuilder.And(filters.ToArray()), "CreatedAt desc", page, pageSize);
        return _apiClient.GetODataListAsync<ExamItemViewModel>(endpoint);
    }

    public Task<ApiResult<ExamItemViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<ExamItemViewModel>($"api/exams/{id}");
    }

    public Task<ApiResult<ExamItemViewModel>> CreateAsync(ExamFormViewModel model)
    {
        return _apiClient.PostAsync<ExamFormViewModel, ExamItemViewModel>("api/exams", model);
    }

    public Task<ApiResult<string>> UpdateAsync(ExamFormViewModel model)
    {
        return _apiClient.PutAsync($"api/exams/{model.Id}", model);
    }

    public Task<ApiResult<string>> DeleteAsync(int id)
    {
        return _apiClient.DeleteAsync($"api/exams/{id}");
    }

    public Task<ApiResult<ExamQuestionsViewModel>> GetQuestionsAsync(int id)
    {
        return _apiClient.GetAsync<ExamQuestionsViewModel>($"api/exams/{id}/questions");
    }

    public Task<ApiResult<string>> UpdateQuestionsAsync(int id, UpdateExamQuestionsViewModel model)
    {
        return _apiClient.PutAsync($"api/exams/{id}/questions", model);
    }

    public Task<ApiResult<PublishExamViewModel>> PublishAsync(int id)
    {
        return _apiClient.PostEmptyAsync<PublishExamViewModel>($"api/exams/{id}/publish");
    }

    public Task<ApiResult<List<ExamAssignmentViewModel>>> GetAssignmentsAsync(int id)
    {
        return _apiClient.GetAsync<List<ExamAssignmentViewModel>>($"api/exams/{id}/assignments");
    }

    public Task<ApiResult<ExamAssignmentViewModel>> AssignClassAsync(int id, AssignExamToClassViewModel model)
    {
        return _apiClient.PostAsync<AssignExamToClassViewModel, ExamAssignmentViewModel>($"api/exams/{id}/assignments", model);
    }

    public Task<ApiResult<string>> RemoveAssignmentAsync(int id, int assignmentId)
    {
        return _apiClient.DeleteAsync($"api/exams/{id}/assignments/{assignmentId}");
    }
}
