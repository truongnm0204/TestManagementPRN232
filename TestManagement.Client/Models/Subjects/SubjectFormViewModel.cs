using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Subjects;

public class SubjectFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã môn học.")]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên môn học.")]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string Status { get; set; } = "Active";
}
