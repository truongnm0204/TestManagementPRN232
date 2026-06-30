using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.Questions;

namespace TestManagement.Client.Services;

public class QuestionService
{
    private readonly ApiClient _apiClient;

    public QuestionService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<QuestionItemViewModel>>> GetListAsync(
        string? keyword,
        int? subjectId,
        string? difficulty,
        string? status,
        int page,
        int pageSize,
        int? createdByUserId = null)
    {
        var filters = BuildFilters(keyword, subjectId, difficulty, status, createdByUserId);
        var endpoint = ODataQueryBuilder.Build("api/questions", ODataQueryBuilder.And(filters.ToArray()), "CreatedAt desc", page, pageSize);
        return _apiClient.GetODataListAsync<QuestionItemViewModel>(endpoint);
    }

    public async Task<List<SubjectPackageViewModel>> GetMySubjectPackagesAsync(int teacherId)
    {
        var filters = new List<string> { $"CreatedByUserId eq {teacherId}" };
        var endpoint = ODataQueryBuilder.Build("api/questions", ODataQueryBuilder.And(filters.ToArray()), "SubjectName", 1, 1000);
        var result = await _apiClient.GetODataListAsync<QuestionItemViewModel>(endpoint);
        if (!result.Success || result.Data == null)
            return new List<SubjectPackageViewModel>();

        return result.Data.Items
            .GroupBy(q => q.SubjectId)
            .Select(g => new SubjectPackageViewModel
            {
                SubjectId = g.Key,
                SubjectCode = g.First().SubjectCode,
                SubjectName = g.First().SubjectName,
                QuestionCount = g.Count()
            })
            .OrderBy(p => p.SubjectName)
            .ToList();
    }

    private static List<string> BuildFilters(string? keyword, int? subjectId, string? difficulty, string? status, int? createdByUserId)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
            filters.Add(ODataQueryBuilder.Contains("Content", keyword));
        if (subjectId.HasValue && subjectId.Value > 0)
            filters.Add($"SubjectId eq {subjectId.Value}");
        if (!string.IsNullOrWhiteSpace(difficulty))
            filters.Add($"Difficulty eq '{ODataQueryBuilder.EscapeValue(difficulty)}'");
        if (!string.IsNullOrWhiteSpace(status))
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        if (createdByUserId.HasValue)
            filters.Add($"CreatedByUserId eq {createdByUserId.Value}");
        return filters;
    }

    public Task<ApiResult<QuestionItemViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<QuestionItemViewModel>($"api/questions/{id}");
    }

    public Task<ApiResult<string>> CreateAsync(QuestionFormViewModel model)
    {
        PrepareOptions(model);
        return _apiClient.PostAsync("api/questions", model);
    }

    public Task<ApiResult<string>> UpdateAsync(QuestionFormViewModel model)
    {
        PrepareOptions(model);
        return _apiClient.PutAsync($"api/questions/{model.Id}", model);
    }

    public Task<ApiResult<string>> DeleteAsync(int id)
    {
        return _apiClient.DeleteAsync($"api/questions/{id}");
    }

    private static void PrepareOptions(QuestionFormViewModel model)
    {
        for (var i = 0; i < model.Options.Count; i++)
        {
            model.Options[i].IsCorrect = i == model.CorrectOptionIndex;
        }

        model.Options = model.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Content))
            .Select((option, index) =>
            {
                option.SortOrder = index + 1;
                return option;
            })
            .ToList();
    }
}
