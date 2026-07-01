namespace TestManagement.Client.Models.ExamAttempts;

// Body gửi lên API khi student chọn/đổi đáp án (auto-save)
public class SaveAnswerRequestModel
{
    public int QuestionId { get; set; }

    // null = bỏ chọn (xóa đáp án)
    public int? SelectedOptionId { get; set; }
}
