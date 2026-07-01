namespace TestManagement.BAL.DTOs.ExamAttempts;

// Một entry trong lịch sử đổi đáp án (lưu trong StudentAnswer.AnswerHistoryJson)
public class AnswerHistoryEntry
{
    public DateTime At { get; set; }
    public int? From { get; set; }  // optionId cũ (null = lần chọn đầu tiên)
    public int? To { get; set; }    // optionId mới (null = bỏ chọn)
}
