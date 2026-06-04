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
        int pageSize)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Contains("Content", keyword));
        }

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            filters.Add($"SubjectId eq {subjectId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(difficulty))
        {
            filters.Add($"Difficulty eq '{ODataQueryBuilder.EscapeValue(difficulty)}'");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        }

        var endpoint = ODataQueryBuilder.Build("api/questions", ODataQueryBuilder.And(filters.ToArray()), "CreatedAt desc", page, pageSize);
        return _apiClient.GetODataListAsync<QuestionItemViewModel>(endpoint);
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
