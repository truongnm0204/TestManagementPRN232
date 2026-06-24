using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.ExamAssignments
{
    public class CreateAssignmentRequest
    {
        [Required(ErrorMessage = "Vui lòng chọn đề thi.")]
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp học để giao đề.")]
        public int ClassId { get; set; }
    }
}