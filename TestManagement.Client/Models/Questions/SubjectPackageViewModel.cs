namespace TestManagement.Client.Models.Questions;

public class SubjectPackageViewModel
{
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int QuestionCount { get; set; }
}
