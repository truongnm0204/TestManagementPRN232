using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Exams;

public class ExamFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên đề thi.")]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Range(1, 600, ErrorMessage = "Thời gian làm bài phải từ 1 đến 600 phút.")]
    public int DurationMinutes { get; set; } = 45;

    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }

    [Range(1, 10, ErrorMessage = "Số lần làm bài phải từ 1 đến 10.")]
    public int AttemptLimit { get; set; } = 1;

    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public bool ShowResultsImmediately { get; set; }
    public bool ShowCorrectAnswers { get; set; }
}
