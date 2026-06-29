using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class UpdateExamQuestionsRequest
    {
        [MinLength(1, ErrorMessage = "Đề thi phải có ít nhất một câu hỏi.")]
        public List<ExamQuestionItemRequest> Items { get; set; } = new();
    }
}
