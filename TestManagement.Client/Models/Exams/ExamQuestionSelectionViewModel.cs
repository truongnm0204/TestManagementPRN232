namespace TestManagement.Client.Models.Exams;

public class ExamQuestionSelectionViewModel
{
    public int QuestionId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public int OptionCount { get; set; }
    public bool IsSelected { get; set; }
    public decimal Points { get; set; } = 1;
    public int SortOrder { get; set; }
}
