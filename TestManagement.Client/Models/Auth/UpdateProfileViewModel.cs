using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Auth;

public class UpdateProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
