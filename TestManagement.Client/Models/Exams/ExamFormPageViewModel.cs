using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Models.Exams;

public class ExamFormPageViewModel
{
    public ExamFormViewModel Form { get; set; } = new();
    public List<SelectOptionViewModel> Subjects { get; set; } = new();
    public bool IsEdit { get; set; }
}
