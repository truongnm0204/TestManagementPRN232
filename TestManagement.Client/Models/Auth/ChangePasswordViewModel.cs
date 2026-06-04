using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Auth;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
    [MinLength(6, ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
    [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
