namespace TestManagement.Client.Models.Classes;

public class ClassItemViewModel
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
