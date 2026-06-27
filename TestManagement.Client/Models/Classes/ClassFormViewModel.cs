using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Classes;

public class ClassFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã lớp.")]
    [MaxLength(50)]
    public string ClassCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên lớp.")]
    [MaxLength(200)]
    public string ClassName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    public string Status { get; set; } = "Active";
}
