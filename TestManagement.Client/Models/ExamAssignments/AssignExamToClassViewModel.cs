using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.ExamAssignments;

public class AssignExamToClassViewModel
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn lớp học.")]
    public int ClassId { get; set; }
}
