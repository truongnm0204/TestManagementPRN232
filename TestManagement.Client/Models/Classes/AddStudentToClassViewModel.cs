using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Classes;

public class AddStudentToClassViewModel
{
    [Required]
    public int StudentId { get; set; }
}
