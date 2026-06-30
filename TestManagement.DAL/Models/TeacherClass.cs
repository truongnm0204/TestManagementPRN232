using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestManagement.DAL.Models;

public class TeacherClass
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int TeacherId { get; set; }
    public int ClassId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    public User? Teacher { get; set; }
    public Class? Class { get; set; }
}
