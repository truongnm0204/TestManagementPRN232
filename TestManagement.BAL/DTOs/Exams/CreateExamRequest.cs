using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class CreateExamRequest
    {
        [Required]
        public int SubjectId { get; set; }
        [Required, MaxLength(500)]
        public string Title { get; set; } = string.Empty;
        [MaxLength(2000)]
        public string? Description { get; set; }
        [Range(1, 600)]
        public int DurationMinutes { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }
        [Range(1, 10)]
        public int AttemptLimit { get; set; } = 1;
        public bool ShuffleQuestions { get; set; }
        public bool ShuffleOptions { get; set; }
        public bool ShowResultsImmediately { get; set; }
        public bool ShowCorrectAnswers { get; set; }

    }
}
