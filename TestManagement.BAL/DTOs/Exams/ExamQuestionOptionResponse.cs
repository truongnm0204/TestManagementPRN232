using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class ExamQuestionOptionResponse
    {
        public int OptionId { get; set; }
        public string Label { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsCorrect { get; set; }
    }
}
