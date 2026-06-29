namespace TestManagement.Client.Models.Exams;

public class ExamComposeViewModel
{
    public ExamItemViewModel Exam { get; set; } = new();
    public ExamQuestionsViewModel SelectedQuestions { get; set; } = new();
    public List<ExamQuestionSelectionViewModel> CandidateQuestions { get; set; } = new();

    public bool CanEdit => Exam.CanEdit;
}
