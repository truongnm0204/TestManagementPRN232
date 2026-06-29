using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Exams;

public class ExamQuestionItemFormViewModel
{
    [Required]
    public int QuestionId { get; set; }

    [Range(0.1, 100)]
    public decimal Points { get; set; } = 1;

    public int SortOrder { get; set; }
}
