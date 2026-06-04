using System.ComponentModel.DataAnnotations;

namespace TestManagement.DAL.Models;

public class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }
    public Question? Question { get; set; }

    [Required, MaxLength(10)]
    public string Label { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
