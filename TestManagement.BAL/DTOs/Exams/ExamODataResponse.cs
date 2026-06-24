using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestManagement.BAL.DTOs.Exams
{
    public class ExamODataResponse
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public string SubjectCode { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;

        public int CreatedBy { get; set; }
        public string CreatorName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableTo { get; set; }

        public int AttemptLimit { get; set; }

        public bool IsPublished { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
