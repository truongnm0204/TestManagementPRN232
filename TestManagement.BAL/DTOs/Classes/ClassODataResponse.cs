namespace TestManagement.BAL.DTOs.Classes;

public class ClassODataResponse
{
    public int Id { get; set; }
    public string ClassCode { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
