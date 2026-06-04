namespace TestManagement.BAL.DTOs.Subjects;

public class SubjectODataResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
