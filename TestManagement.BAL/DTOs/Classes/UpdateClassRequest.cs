using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Classes;

public class UpdateClassRequest
{
    [Required, MaxLength(200)]
    public string ClassName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";
}
