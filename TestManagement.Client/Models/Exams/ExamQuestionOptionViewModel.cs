namespace TestManagement.Client.Models.Exams;

public class ExamQuestionOptionViewModel
{
    public int OptionId { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCorrect { get; set; }
}
