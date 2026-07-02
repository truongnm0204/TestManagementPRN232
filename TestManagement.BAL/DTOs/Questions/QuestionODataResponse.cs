namespace TestManagement.BAL.DTOs.Questions;

public class QuestionODataResponse
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public int? TopicId { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int? CreatedByUserId { get; set; }

    // Danh sách đáp án để hiển thị khi giáo viên chọn câu hỏi cho đề thi
    public List<QuestionOptionResponse> Options { get; set; } = new();
}
