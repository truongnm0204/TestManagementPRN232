using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class ExamQuestionItemRequest
    {
        [Required]
        public int QuestionId { get; set; }

        [Range(0.1, 100)]
        public decimal Points { get; set; } = 1;

        public int SortOrder { get; set; }
    }
}
