using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TestManagement.BAL.DTOs.Auth;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.BAL.Settings;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly JwtSettings _jwtSettings;

    public AuthService(IUserRepository userRepository, IOptions<JwtSettings> jwtOptions)
    {
        _userRepository = userRepository;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<ServiceResult<LoginResponse>> LoginAsync(LoginRequest request )
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !user.IsActive)
        {
            return ServiceResult<LoginResponse>.Fail("Email hoặc mật khẩu không đúng.");
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return ServiceResult<LoginResponse>.Fail("Email hoặc mật khẩu không đúng.");
        }

        user.LastLoginAt = DateTime.Now;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);
        var token = GenerateToken(user, expiresAt);

        return ServiceResult<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            User = MapToCurrentUser(user)
        });
    }

    public async Task<ServiceResult<LoginResponse>> RegisterAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
            return ServiceResult<LoginResponse>.Fail("Email đã được sử dụng.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            PhoneNumber = request.PhoneNumber,
            Role = "Student",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);
        return ServiceResult<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = GenerateToken(user, expiresAt),
            ExpiresAt = expiresAt,
            User = MapToCurrentUser(user)
        });
    }

    public async Task<ServiceResult<LoginResponse>> GoogleLoginAsync(GoogleLoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
        {
            user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                Role = "Student",
                IsActive = true,
                CreatedAt = DateTime.Now
            };
            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
        }
        else if (!user.IsActive)
        {
            return ServiceResult<LoginResponse>.Fail("Tài khoản đã bị khóa.");
        }

        user.LastLoginAt = DateTime.Now;
        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresMinutes);
        return ServiceResult<LoginResponse>.Ok(new LoginResponse
        {
            AccessToken = GenerateToken(user, expiresAt),
            ExpiresAt = expiresAt,
            User = MapToCurrentUser(user)
        });
    }

    public async Task<ServiceResult<CurrentUserResponse>> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetActiveByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return ServiceResult<CurrentUserResponse>.Fail("Không tìm thấy người dùng.");
        }

        return ServiceResult<CurrentUserResponse>.Ok(MapToCurrentUser(user));
    }

    public async Task<ServiceResult<CurrentUserResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetActiveByIdAsync(userId);
        if (user == null)
            return ServiceResult<CurrentUserResponse>.Fail("Không tìm thấy người dùng.");

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.Now;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult<CurrentUserResponse>.Ok(MapToCurrentUser(user));
    }

    public async Task<ServiceResult> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
        {
            return ServiceResult.Fail("Mật khẩu xác nhận không khớp.");
        }

        var user = await _userRepository.GetActiveByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            return ServiceResult.Fail("Không tìm thấy người dùng.");
        }

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);

        if (!isCurrentPasswordValid)
        {
            return ServiceResult.Fail("Mật khẩu hiện tại không đúng.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.Now;

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private string GenerateToken(User user, DateTime expiresAt)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static CurrentUserResponse MapToCurrentUser(User user)
    {
        return new CurrentUserResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}
