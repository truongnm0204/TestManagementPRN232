using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Classes;

public class AddTeacherRequest
{
    [Required]
    public int TeacherId { get; set; }
}
