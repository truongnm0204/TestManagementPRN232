namespace TestManagement.BAL.DTOs.ExamAssignments;

public class ExamAssignmentResponse
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int AssignedBy { get; set; }
    public string AssignerName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
