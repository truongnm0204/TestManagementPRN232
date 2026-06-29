using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class ExamQuestionsResponse
    {
        public int ExamId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalPoints { get; set; }
        public List<ExamQuestionItemResponse> Items { get; set; } = new();
    }

}
