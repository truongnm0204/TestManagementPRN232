using Microsoft.AspNetCore.Http;
using TestManagement.Client.Models.Classes;
using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Services;

public class ClassService
{
    private readonly ApiClient _apiClient;

    public ClassService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<ODataListResult<ClassItemViewModel>>> GetListAsync(string? keyword, string? status, int page, int pageSize)
    {
        var filters = BuildFilters(keyword, status);
        var endpoint = ODataQueryBuilder.Build("api/classes", ODataQueryBuilder.And(filters.ToArray()), "ClassName", page, pageSize);
        return _apiClient.GetODataListAsync<ClassItemViewModel>(endpoint);
    }

    public Task<ApiResult<ODataListResult<ClassItemViewModel>>> GetMyClassesAsync(string? keyword, string? status, int page, int pageSize)
    {
        var filters = BuildFilters(keyword, status);
        var endpoint = ODataQueryBuilder.Build("api/classes/my", ODataQueryBuilder.And(filters.ToArray()), "ClassName", page, pageSize);
        return _apiClient.GetODataListAsync<ClassItemViewModel>(endpoint);
    }

    public Task<ApiResult<ClassDetailViewModel>> GetByIdAsync(int id)
    {
        return _apiClient.GetAsync<ClassDetailViewModel>($"api/classes/{id}");
    }

    public Task<ApiResult<string>> CreateAsync(ClassFormViewModel model)
    {
        return _apiClient.PostAsync("api/classes", model);
    }

    public Task<ApiResult<string>> UpdateAsync(ClassFormViewModel model)
    {
        return _apiClient.PutAsync($"api/classes/{model.Id}", model);
    }

    public Task<ApiResult<string>> SetStatusAsync(int id, string status)
    {
        return _apiClient.PatchAsync($"api/classes/{id}/status", status);
    }

    public async Task<List<ClassItemViewModel>> GetActiveClassesAsync()
    {
        var result = await GetListAsync(null, "Active", 1, 200);
        return result.Data?.Items ?? new List<ClassItemViewModel>();
    }

    public Task<ApiResult<string>> AddStudentAsync(int classId, int studentId)
    {
        return _apiClient.PostAsync($"api/classes/{classId}/students", new { StudentId = studentId });
    }

    public Task<ApiResult<string>> RemoveStudentAsync(int classId, int studentId)
    {
        return _apiClient.DeleteAsync($"api/classes/{classId}/students/{studentId}");
    }

    public Task<ApiResult<string>> LeaveClassAsync(int classId)
    {
        return _apiClient.PostAsync($"api/classes/{classId}/leave", new { });
    }

    public Task<ApiResult<string>> DissolveClassAsync(int classId)
    {
        return _apiClient.DeleteAsync($"api/classes/{classId}/dissolve");
    }

    public Task<ApiResult<string>> AddTeacherAsync(int classId, int teacherId)
    {
        return _apiClient.PostAsync($"api/classes/{classId}/teachers", new { TeacherId = teacherId });
    }

    public Task<ApiResult<string>> RemoveTeacherAsync(int classId, int teacherId)
    {
        return _apiClient.DeleteAsync($"api/classes/{classId}/teachers/{teacherId}");
    }

    public Task<ApiResult<ImportStudentsResultViewModel>> ImportStudentsAsync(int classId, IFormFile file)
    {
        return _apiClient.PostFileAsync<ImportStudentsResultViewModel>($"api/classes/{classId}/students/import", file);
    }

    private static List<string> BuildFilters(string? keyword, string? status)
    {
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            filters.Add(ODataQueryBuilder.Or(
                ODataQueryBuilder.Contains("ClassCode", keyword),
                ODataQueryBuilder.Contains("ClassName", keyword)));
        }
        if (!string.IsNullOrWhiteSpace(status))
            filters.Add($"Status eq '{ODataQueryBuilder.EscapeValue(status)}'");
        return filters;
    }
}
