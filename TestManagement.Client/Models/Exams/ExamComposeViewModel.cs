using TestManagement.Client.Models.Classes;
using TestManagement.Client.Models.ExamAssignments;

namespace TestManagement.Client.Models.Exams;

public class ExamComposeViewModel
{
    public ExamItemViewModel Exam { get; set; } = new();
    public ExamQuestionsViewModel SelectedQuestions { get; set; } = new();
    public List<ExamQuestionSelectionViewModel> CandidateQuestions { get; set; } = new();
    public List<ExamAssignmentViewModel> Assignments { get; set; } = new();
    public List<ClassItemViewModel> ActiveClasses { get; set; } = new();
    public AssignExamToClassViewModel AssignForm { get; set; } = new();

    public bool CanEdit => Exam.CanEdit;
    public bool CanAssign => Exam.IsPublished && Exam.Status == "Published";
}
