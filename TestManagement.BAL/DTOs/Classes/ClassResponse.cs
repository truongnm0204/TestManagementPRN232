namespace TestManagement.BAL.DTOs.Classes;

public class ClassResponse
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<StudentInClassResponse> Students { get; set; } = new();
    public TeacherInClassResponse? Teacher { get; set; }
}

public class TeacherInClassResponse
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class StudentInClassResponse
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime JoinedAt { get; set; }
}
