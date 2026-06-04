using TestManagement.Client.Models.Common;
using TestManagement.Client.Models.Users;

namespace TestManagement.Client.Services;

public class UserService
{
    private readonly ApiClient _apiClient;

    public UserService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<UserItemViewModel>>> GetListAsync(string? keyword, string? role, bool? isActive, int page, int pageSize)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Or(
                ODataQueryBuilder.Contains("FullName", keyword),
                ODataQueryBuilder.Contains("Email", keyword)));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            filters.Add($"Role eq '{ODataQueryBuilder.EscapeValue(role)}'");
        }

        if (isActive.HasValue)
        {
            filters.Add($"IsActive eq {isActive.Value.ToString().ToLowerInvariant()}");
        }

        var endpoint = ODataQueryBuilder.Build("api/users", ODataQueryBuilder.And(filters.ToArray()), "FullName", page, pageSize);
        return _apiClient.GetODataListAsync<UserItemViewModel>(endpoint);
    }

    public Task<ApiResult<UserItemViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<UserItemViewModel>($"api/users/{id}");
    }

    public Task<ApiResult<string>> CreateAsync(UserFormViewModel model)
    {
        return _apiClient.PostAsync("api/users", model);
    }

    public Task<ApiResult<string>> UpdateAsync(UserFormViewModel model)
    {
        return _apiClient.PutAsync($"api/users/{model.Id}", model);
    }

    public Task<ApiResult<string>> UpdateStatusAsync(int id, bool isActive)
    {
        return _apiClient.PatchAsync($"api/users/{id}/status", new UserStatusViewModel { IsActive = isActive });
    }

    public Task<ApiResult<string>> DeleteAsync(int id)
    {
        return _apiClient.DeleteAsync($"api/users/{id}");
    }
}
