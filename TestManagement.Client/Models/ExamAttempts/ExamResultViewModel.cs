namespace TestManagement.Client.Models.ExamAttempts;

// Chi tiết một câu trong kết quả
public class AnswerDetailViewModel
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

    // Chỉ có dữ liệu khi exam cho phép ShowCorrectAnswers
    public List<int> CorrectOptionIds { get; set; } = new();
    public List<TakeOptionViewModel> Options { get; set; } = new();
}

// Khớp với SubmitAttemptResponse từ API — dùng cho cả Submit và Result
public class ExamResultViewModel
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
    public List<AnswerDetailViewModel> Details { get; set; } = new();
}
