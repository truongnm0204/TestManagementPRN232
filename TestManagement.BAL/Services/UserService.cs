using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Users;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class UserService : IUserService
{
    private static readonly string[] ValidRoles = { "Admin", "Staff", "Student" };
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IQueryable<UserODataResponse> GetODataQueryable()
    {
        return _userRepository.GetQueryable().Select(user => new UserODataResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        });
    }

    public async Task<UserResponse?> GetByIdAsync(int id)
    {
        var user = await _userRepository.GetActiveByIdAsync(id);
        return user == null ? null : MapToResponse(user);
    }

    public async Task<ServiceResult<UserResponse>> CreateAsync(CreateUserRequest request)
    {
        if (!IsValidRole(request.Role))
        {
            return ServiceResult<UserResponse>.Fail("Role không hợp lệ.");
        }

        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return ServiceResult<UserResponse>.Fail("Email đã tồn tại.");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            AvatarUrl = request.AvatarUrl,
            Role = request.Role,
            IsActive = request.IsActive,
            CreatedAt = DateTime.Now
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult<UserResponse>.Ok(MapToResponse(user));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateUserRequest request)
    {
        if (!IsValidRole(request.Role))
        {
            return ServiceResult.Fail("Role không hợp lệ.");
        }

        var user = await _userRepository.GetActiveByIdAsync(id);

        if (user == null)
        {
            return ServiceResult.Fail("Không tìm thấy người dùng.");
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.AvatarUrl = request.AvatarUrl;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.Now;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> UpdateStatusAsync(int id, bool isActive, int currentUserId)
    {
        if (id == currentUserId && !isActive)
        {
            return ServiceResult.Fail("Không thể khóa chính tài khoản đang đăng nhập.");
        }

        var user = await _userRepository.GetActiveByIdAsync(id);

        if (user == null)
        {
            return ServiceResult.Fail("Không tìm thấy người dùng.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.Now;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id, int currentUserId)
    {
        if (id == currentUserId)
        {
            return ServiceResult.Fail("Không thể xóa chính tài khoản đang đăng nhập.");
        }

        var user = await _userRepository.GetActiveByIdAsync(id);

        if (user == null)
        {
            return ServiceResult.Fail("Không tìm thấy người dùng.");
        }

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.Now;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private static bool IsValidRole(string role)
    {
        return ValidRoles.Contains(role);
    }

    private static UserResponse MapToResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}
