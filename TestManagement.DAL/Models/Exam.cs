using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TestManagement.DAL.Models
{
    public class Exam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int SubjectId { get; set; }
        public int CreatedBy { get; set; }
        [Required]
        [MaxLength(500)]
        public string Title { get; set; } = String.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        
        
        
        [MaxLength(20)]
        public string Status { get; set; } = "Draft";
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }
        public int AttemptLimit { get; set; } = 1;
        public bool ShuffleQuestions { get; set; } 
        public bool ShuffleOptions { get; set; }
        public bool ShowResultsImmediately { get; set; }
        public bool ShowCorrectAnswers { get; set; }

        public string? QuestionItemsJson { get; set; }
        public string? PublishedSnapshotJson { get; set; }
        public string? AnswerKeyJson { get; set; }
        public string? SettingsJson { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Subject? Subject { get; set; }
        public User? Creator { get; set; }
        public ICollection<ExamAssignment> ExamAssignments { get; set; } = new List<ExamAssignment>();
        public ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();
    }



    
}
