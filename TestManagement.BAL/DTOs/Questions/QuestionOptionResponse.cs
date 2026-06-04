namespace TestManagement.BAL.DTOs.Questions;

public class QuestionOptionResponse
{
    public int Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int SortOrder { get; set; }
}
