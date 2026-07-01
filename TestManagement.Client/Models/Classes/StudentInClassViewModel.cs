namespace TestManagement.Client.Models.Classes;

public class StudentInClassViewModel
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime JoinedAt { get; set; }
}
