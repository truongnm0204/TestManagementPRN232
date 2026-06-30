namespace TestManagement.Client.Models.Questions;

public class QuestionItemViewModel
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<QuestionOptionFormViewModel> Options { get; set; } = new();
}
