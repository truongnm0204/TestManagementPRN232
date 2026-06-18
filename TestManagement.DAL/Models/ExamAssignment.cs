using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestManagement.DAL.Models
{
    public class ExamAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int ClassId { get; set; }
        public int AssignedBy { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        // navigation properties
        public Exam? Exam { get; set; }
        public Class? Class { get; set; }
        public User? Assigner { get; set; }

    }
}