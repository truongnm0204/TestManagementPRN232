namespace TestManagement.Client.Models.ExamAttempts;

public class MyExamItemViewModel
{
    public int ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public int AttemptLimit { get; set; }
    public int AttemptsDone { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
    public string? LatestAttemptStatus { get; set; }
    public int? LatestAttemptId { get; set; }
    public bool CanStart { get; set; }

    public bool HasInProgress => LatestAttemptStatus == "InProgress";
    public bool HasSubmitted => LatestAttemptStatus == "Submitted";
    public int AttemptsLeft => AttemptLimit - AttemptsDone;
}
