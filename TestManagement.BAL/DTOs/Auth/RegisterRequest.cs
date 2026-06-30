using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Auth;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
