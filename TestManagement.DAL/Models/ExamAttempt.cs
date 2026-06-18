
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace TestManagement.DAL.Models
{

    public class ExamAttempt
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ExamId { get; set; }
        public int StudentId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? TotalScore { get; set; }

        public int CorrectCount { get; set; }
        public int WrongCount { get; set; }

        [MaxLength(20)]
        public string Status { get; set; } = "InProgress";

        public string? QuestionSnapshotJson { get; set; }
        public string? AttemptMetaJson { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public Exam? Exam { get; set; }
        public User? Student { get; set; }
        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
    }
}