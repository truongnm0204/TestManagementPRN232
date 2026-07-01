using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string FullName { get; set; } = string.Empty;
}
