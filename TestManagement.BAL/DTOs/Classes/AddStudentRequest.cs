using System.ComponentModel.DataAnnotations;

namespace TestManagement.BAL.DTOs.Classes;

public class AddStudentRequest
{
    [Required]
    public int StudentId { get; set; }
}
