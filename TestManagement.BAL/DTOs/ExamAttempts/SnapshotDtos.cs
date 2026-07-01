namespace TestManagement.BAL.DTOs.ExamAttempts;

public class SnapshotOptionDto
{
    public int OptionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class SnapshotQuestionDto
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public decimal Points { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<SnapshotOptionDto> Options { get; set; } = new();
}

public class SnapshotDto
{
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public List<SnapshotQuestionDto> Questions { get; set; } = new();
}

public class AnswerKeyItemDto
{
    public int QuestionId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public List<int> CorrectOptionIds { get; set; } = new();
    public decimal Points { get; set; }
}
