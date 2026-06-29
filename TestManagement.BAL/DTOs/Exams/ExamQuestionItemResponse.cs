using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class ExamQuestionItemResponse
    {
        public int QuestionId { get; set; }
        public int SortOrder { get; set; }
        public decimal Points { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public List<ExamQuestionOptionResponse> Options { get; set; } = new();
    }
}
