using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.Subjects;

namespace TestManagement.Client.Services;

public class SubjectService
{
    private readonly ApiClient _apiClient;

    public SubjectService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<SubjectItemViewModel>>> GetListAsync(string? keyword, string? status, int page, int pageSize)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Or(
                ODataQueryBuilder.Contains("Code", keyword),
                ODataQueryBuilder.Contains("Name", keyword)));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        }

        var endpoint = ODataQueryBuilder.Build("api/subjects", ODataQueryBuilder.And(filters.ToArray()), "Name", page, pageSize);
        return _apiClient.GetODataListAsync<SubjectItemViewModel>(endpoint);
    }

    public Task<ApiResult<SubjectItemViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<SubjectItemViewModel>($"api/subjects/{id}");
    }

    public Task<ApiResult<string>> CreateAsync(SubjectFormViewModel model)
    {
        return _apiClient.PostAsync("api/subjects", model);
    }

    public Task<ApiResult<string>> UpdateAsync(SubjectFormViewModel model)
    {
        return _apiClient.PutAsync($"api/subjects/{model.Id}", model);
    }

    public Task<ApiResult<string>> DeleteAsync(int id)
    {
        return _apiClient.DeleteAsync($"api/subjects/{id}");
    }

    public async Task<List<SelectOptionViewModel>> GetSubjectOptionsAsync()
    {
        var result = await GetListAsync(null, "Active", 1, 100);
        return result.Data?.Items
            .Select(subject => new SelectOptionViewModel
            {
                Value = subject.Id.ToString(),
                Text = $"{subject.Code} - {subject.Name}"
            })
            .ToList() ?? new List<SelectOptionViewModel>();
    }
}
