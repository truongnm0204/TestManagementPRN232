using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TestManagement.DAL.Models
{
    public class Class
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string ClassCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(200)]
        public string ClassName { get; set; } = string.Empty;
        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "Active";
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User? Creator { get; set; }
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<TeacherClass> TeacherClasses { get; set; } = new List<TeacherClass>();
        public ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();
    }
}
