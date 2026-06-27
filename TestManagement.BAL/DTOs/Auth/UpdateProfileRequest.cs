using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [MaxLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
