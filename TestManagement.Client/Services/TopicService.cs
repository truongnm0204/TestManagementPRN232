using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.Topics;

namespace TestManagement.Client.Services;

public class TopicService
{
    private readonly ApiClient _apiClient;

    public TopicService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<TopicItemViewModel>>> GetListAsync(
        string? keyword,
        int? subjectId,
        string? status,
        int page,
        int pageSize)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Contains("Name", keyword));
        }

        if (subjectId.HasValue && subjectId.Value > 0)
        {
            filters.Add($"SubjectId eq {subjectId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        }

        var endpoint = ODataQueryBuilder.Build("api/topics", ODataQueryBuilder.And(filters.ToArray()), "DisplayOrder,Name", page, pageSize);
        return _apiClient.GetODataListAsync<TopicItemViewModel>(endpoint);
    }

    public Task<ApiResult<TopicItemViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<TopicItemViewModel>($"api/topics/{id}");
    }

    public Task<ApiResult<string>> CreateAsync(TopicFormViewModel model)
    {
        return _apiClient.PostAsync("api/topics", model);
    }

    public Task<ApiResult<string>> UpdateAsync(TopicFormViewModel model)
    {
        return _apiClient.PutAsync($"api/topics/{model.Id}", model);
    }

    public Task<ApiResult<string>> DeleteAsync(int id)
    {
        return _apiClient.DeleteAsync($"api/topics/{id}");
    }
}
