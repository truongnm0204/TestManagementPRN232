namespace TestManagement.Client.Models.ExamAttempts;

// Một option của câu hỏi (KHÔNG bao giờ chứa IsCorrect khi đang làm bài)
public class TakeOptionViewModel
{
    public int OptionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

// Một câu hỏi trong đề (đã shuffle, lấy từ QuestionSnapshotJson)
public class TakeQuestionViewModel
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public decimal Points { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<TakeOptionViewModel> Options { get; set; } = new();
}

// Khớp với StartAttemptResponse từ API — dùng cho màn Take
public class TakeExamViewModel
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? DeadlineAt { get; set; }
    public bool ShuffleOptions { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public List<TakeQuestionViewModel> Questions { get; set; } = new();

    // questionId -> selectedOptionId (đáp án đã lưu trước đó, dùng khi resume)
    public Dictionary<int, int> SavedAnswers { get; set; } = new();
}
