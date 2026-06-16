using System.ComponentModel.DataAnnotations;

namespace TestManagement.DAL.Models;

public class Question
{
    public int Id { get; set; }

    public int SubjectId { get; set; }
    public int? TopicId { get; set; }
    public Subject? Subject { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    public string? ContentJson { get; set; }

    [MaxLength(2000)]
    public string? Explanation { get; set; }

    [Required, MaxLength(30)]
    public string Difficulty { get; set; } = "Easy";
    [MaxLength(30)]
    public string QuestionType { get; set; } = "SingleChoice";
    [MaxLength(20)]
    public string SourceType { get; set; } = "Manual";
    [Required, MaxLength(30)]
    public string Status { get; set; } = "Active";

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public int? CreatedByUserId { get; set; }
    public Topic? Topic { get; set; }
    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}
