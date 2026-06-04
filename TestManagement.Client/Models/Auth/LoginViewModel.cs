using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = string.Empty;
}
