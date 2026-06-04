using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Questions;

public class UpdateQuestionRequest
{
    [Required]
    public int SubjectId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    [Required, MaxLength(30)]
    public string Difficulty { get; set; } = "Easy";

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Active";

    [MinLength(2)]
    public List<QuestionOptionRequest> Options { get; set; } = new();
}
