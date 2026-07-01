using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.ExamAssignments;

public class AssignExamRequest
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Lớp học không hợp lệ.")]
    public int ClassId { get; set; }
}
