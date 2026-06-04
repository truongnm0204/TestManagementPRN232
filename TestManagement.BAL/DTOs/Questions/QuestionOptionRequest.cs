using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Questions;

public class QuestionOptionRequest
{
    [Required, MaxLength(10)]
    public string Label { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
