using TestManagement.BAL.DTOs.Auth;
using TestManagement.BAL.DTOs.Common;

namespace TestManagement.BAL.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request);
    Task<ServiceResult<CurrentUserResponse>> GetCurrentUserAsync(int userId);
    Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
