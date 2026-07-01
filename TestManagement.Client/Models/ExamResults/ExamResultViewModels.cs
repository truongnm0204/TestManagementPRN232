namespace TestManagement.Client.Models.ExamResults;

// Một cặp (đề thi × lớp) đã giao — màn Index của Teacher
public class ExamAssignmentSummaryViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

// Một dòng kết quả của 1 student
public class ClassResultRowViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? AttemptId { get; set; }
    public string Status { get; set; } = "NotStarted";
    public decimal? TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public bool HasSubmitted => Status == "Submitted";
    public bool HasInProgress => Status == "InProgress";
    public bool NotStarted => Status == "NotStarted";
}

// Bảng kết quả cả lớp cho một đề
public class ClassResultViewModel
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public int QuestionCount { get; set; }
    public List<ClassResultRowViewModel> Rows { get; set; } = new();
}

// Một lần đổi đáp án (đã map sang label)
public class AnswerHistoryChangeViewModel
{
    public DateTime At { get; set; }
    public string? FromLabel { get; set; }
    public string? ToLabel { get; set; }
}

// Lịch sử đổi đáp án của một câu hỏi
public class AnswerHistoryViewModel
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<AnswerHistoryChangeViewModel> Changes { get; set; } = new();
}
