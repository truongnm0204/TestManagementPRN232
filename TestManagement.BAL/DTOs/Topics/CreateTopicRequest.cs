using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Topics;

public class CreateTopicRequest
{
    [Required]
    public int SubjectId { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    [Required, MaxLength(20)]
    public string Status { get; set; } = "Active";
}
