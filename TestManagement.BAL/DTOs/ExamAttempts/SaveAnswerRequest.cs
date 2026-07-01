using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.ExamAttempts;

public class SaveAnswerRequest
{
    [Required]
    public int QuestionId { get; set; }

    /// <summary>null = bỏ qua / xóa đáp án đã chọn</summary>
    public int? SelectedOptionId { get; set; }
}
