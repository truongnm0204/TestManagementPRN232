using TestManagement.BAL.DTOs.Auth;
using TestManagement.BAL.DTOs.Common;

namespace TestManagement.BAL.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<LoginResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest request);
    Task<ServiceResult<CurrentUserResponse>> GetCurrentUserAsync(int userId);
    Task<ServiceResult<CurrentUserResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
