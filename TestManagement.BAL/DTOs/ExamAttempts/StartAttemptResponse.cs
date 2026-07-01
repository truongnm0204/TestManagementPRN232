namespace TestManagement.BAL.DTOs.ExamAttempts;

/// <summary>Trả về khi start attempt thành công (cũng dùng để resume)</summary>
public class StartAttemptResponse
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? DeadlineAt { get; set; }   // StartedAt + Duration
    public bool ShuffleOptions { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public List<SnapshotQuestionDto> Questions { get; set; } = new();
    /// <summary>questionId -> selectedOptionId (đã lưu trước đó)</summary>
    public Dictionary<int, int> SavedAnswers { get; set; } = new();
}

/// <summary>Danh sách đề thi dành cho student xem ở trang Index</summary>
public class MyExamResponse
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
    /// <summary>null = chưa làm, "InProgress", "Submitted"</summary>
    public string? LatestAttemptStatus { get; set; }
    public int? LatestAttemptId { get; set; }
    public bool CanStart { get; set; }
}
