namespace TestManagement.Client.Models.Exams;

public class ExamQuestionItemViewModel
{
    public int QuestionId { get; set; }
    public int SortOrder { get; set; }
    public decimal Points { get; set; }
    public string QuestionType { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Difficulty { get; set; } = string.Empty;
    public List<ExamQuestionOptionViewModel> Options { get; set; } = new();
}
