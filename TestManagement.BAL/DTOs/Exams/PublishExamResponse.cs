namespace TestManagement.BAL.DTOs.Exams
{
    public class PublishExamResponse
    {
        public int ExamId { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int QuestionCount { get; set; }
        public decimal TotalPoints { get; set; }
    }
}
