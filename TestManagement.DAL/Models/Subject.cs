using System.ComponentModel.DataAnnotations;

namespace TestManagement.DAL.Models;

public class Subject
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(30)]
    public string Status { get; set; } = "Active";

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
}
