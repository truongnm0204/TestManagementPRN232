using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Questions;

public class QuestionFormViewModel
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn môn học.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi.")]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    [Required]
    public string Difficulty { get; set; } = "Easy";

    [Required]
    public string Status { get; set; } = "Active";

    public int CorrectOptionIndex { get; set; }

    public List<QuestionOptionFormViewModel> Options { get; set; } = new()
    {
        new QuestionOptionFormViewModel { Label = "A", SortOrder = 1 },
        new QuestionOptionFormViewModel { Label = "B", SortOrder = 2 },
        new QuestionOptionFormViewModel { Label = "C", SortOrder = 3 },
        new QuestionOptionFormViewModel { Label = "D", SortOrder = 4 }
    };
}
