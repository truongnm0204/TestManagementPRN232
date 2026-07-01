using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Auth;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [MaxLength(100)]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [MaxLength(20)]
    [Display(Name = "Số điện thoại")]
    public string? PhoneNumber { get; set; }
}
