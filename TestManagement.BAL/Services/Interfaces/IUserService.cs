using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Users;

namespace TestManagement.BAL.Services.Interfaces;

public interface IUserService
{
    IQueryable<UserODataResponse> GetODataQueryable();
    Task<UserResponse?> GetByIdAsync(int id);
    Task<ServiceResult<UserResponse>> CreateAsync(CreateUserRequest request);
    Task<ServiceResult> UpdateAsync(int id, UpdateUserRequest request);
    Task<ServiceResult> UpdateStatusAsync(int id, bool isActive, int currentUserId);
    Task<ServiceResult> DeleteAsync(int id, int currentUserId);
}
