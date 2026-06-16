using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestManagement.DAL.Models
{
    public class StudentAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ExamAttemptId { get; set; }
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }

        [MaxLength(10)]
        public string? SelectedDisplayLabel { get; set; }

        public string? AnswerJson { get; set; }
        public string? AnswerHistoryJson { get; set; }

        public bool? IsCorrect { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        public DateTime? AnsweredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ExamAttempt? ExamAttempt { get; set; }
        public Question? Question { get; set; }
        public QuestionOption? SelectedOption { get; set; }
    }
}