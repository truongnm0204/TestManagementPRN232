namespace TestManagement.BAL.DTOs.ExamAttempts;

public class AnswerDetailItem
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public string Content { get; set; } = string.Empty;
    public string QuestionType { get; set; } = string.Empty;
    public decimal Points { get; set; }
    public int? SelectedOptionId { get; set; }
    public string? SelectedLabel { get; set; }
    public bool? IsCorrect { get; set; }
    public decimal? Score { get; set; }
    /// <summary>Chỉ populate khi ShowCorrectAnswers = true</summary>
    public List<int> CorrectOptionIds { get; set; } = new();
    public List<SnapshotOptionDto> Options { get; set; } = new();
}

public class SubmitAttemptResponse
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public decimal MaxScore { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public int SkippedCount { get; set; }
    public int QuestionCount { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool ShowResultsImmediately { get; set; }
    public bool ShowCorrectAnswers { get; set; }
    public List<AnswerDetailItem> Details { get; set; } = new();
}
