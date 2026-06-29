using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Topics;

public class TopicFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên chủ đề.")]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    [Required]
    public string Status { get; set; } = "Active";
}
