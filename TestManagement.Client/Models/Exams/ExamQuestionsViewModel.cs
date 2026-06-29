namespace TestManagement.Client.Models.Exams;

public class ExamQuestionsViewModel
{
    public int ExamId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int QuestionCount { get; set; }
    public decimal TotalPoints { get; set; }
    public List<ExamQuestionItemViewModel> Items { get; set; } = new();
}
