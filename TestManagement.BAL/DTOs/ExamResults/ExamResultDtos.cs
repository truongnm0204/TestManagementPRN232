namespace TestManagement.BAL.DTOs.ExamResults;

// Một cặp (đề thi × lớp) đã được giao — dùng cho màn Index của Teacher
public class ExamAssignmentSummaryDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}

// Một dòng trong bảng kết quả lớp (1 student)
public class ClassResultRowDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public int? AttemptId { get; set; }
    public string Status { get; set; } = "NotStarted"; // NotStarted | InProgress | Submitted
    public decimal? TotalScore { get; set; }
    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

// Bảng kết quả của cả lớp cho một đề thi
public class ClassResultDto
{
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public decimal MaxScore { get; set; }
    public int QuestionCount { get; set; }
    public List<ClassResultRowDto> Rows { get; set; } = new();
}

// Một lần đổi đáp án (đã map optionId -> label để hiển thị)
public class AnswerHistoryChangeDto
{
    public DateTime At { get; set; }
    public string? FromLabel { get; set; }
    public string? ToLabel { get; set; }
}

// Lịch sử đổi đáp án của một câu hỏi trong attempt
public class AnswerHistoryViewDto
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public string Content { get; set; } = string.Empty;
    public List<AnswerHistoryChangeDto> Changes { get; set; } = new();
}
