using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Users;

public class UpdateUserRequest
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone, MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    [Required, MaxLength(30)]
    public string Role { get; set; } = "Student";

    public bool IsActive { get; set; } = true;
}
