using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Questions;

public class QuestionOptionFormViewModel
{
    [Required]
    [MaxLength(10)]
    public string Label { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung đáp án.")]
    [MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
