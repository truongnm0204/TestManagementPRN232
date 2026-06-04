using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Users;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    public string? Password { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(255)]
    public string? AvatarUrl { get; set; }

    [Required]
    public string Role { get; set; } = "Student";

    public bool IsActive { get; set; } = true;
}
