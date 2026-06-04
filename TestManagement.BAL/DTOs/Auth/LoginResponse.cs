namespace TestManagement.BAL.DTOs.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public CurrentUserResponse User { get; set; } = new();
}
