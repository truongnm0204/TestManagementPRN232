namespace TestManagement.Client.Models.Auth;

public class LoginResponseViewModel
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserViewModel User { get; set; } = new();
}
