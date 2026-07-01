namespace TestManagement.Client.Models.Classes;

public class ClassDetailViewModel
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StudentInClassViewModel> Students { get; set; } = new();
    public TeacherInClassViewModel? Teacher { get; set; }
}

public class TeacherInClassViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime AssignedAt { get; set; }
}
