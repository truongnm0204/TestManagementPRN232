using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Exams;

public class UpdateExamQuestionsViewModel
{
    [MinLength(1, ErrorMessage = "Đề thi phải có ít nhất một câu hỏi.")]
    public List<ExamQuestionItemFormViewModel> Items { get; set; } = new();
}
