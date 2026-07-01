using TestManagement.Client.Models.Auth;
using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public Task<ApiResult<LoginResponseViewModel>> LoginAsync(LoginViewModel model)
    {
        return _apiClient.PostAsync<LoginViewModel, LoginResponseViewModel>("api/auth/login", model);
    }

    public Task<ApiResult<string>> LogoutAsync()
    {
        return _apiClient.PostEmptyAsync("api/auth/logout");
    }

    public Task<ApiResult<string>> UpdateProfileAsync(UpdateProfileViewModel model)
    {
        return _apiClient.PutAsync("api/auth/me", model);
    }

    public Task<ApiResult<CurrentUserViewModel>> GetCurrentUserAsync()
    {
        return _apiClient.GetAsync<CurrentUserViewModel>("api/auth/me");
    }

    public Task<ApiResult<string>> ChangePasswordAsync(ChangePasswordViewModel model)
    {
        return _apiClient.PostAsync("api/auth/change-password", model);
    }
}
