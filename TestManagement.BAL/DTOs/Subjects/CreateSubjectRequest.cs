using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Subjects;

public class CreateSubjectRequest
{
    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Active";
}
