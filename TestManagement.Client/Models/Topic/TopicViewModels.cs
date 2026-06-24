// Models/Topics/TopicViewModels.cs
using System.ComponentModel.DataAnnotations;

namespace TestManagement.Client.Models.Topics;

// Class dùng để hiển thị trên danh sách Index (Map khớp với dữ liệu OData từ API)
public class TopicItemViewModel
{
    public int Id { get; set; }
    public int SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string Status { get; set; } = "Active";
    public int QuestionCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Class dùng cho Form submit dữ liệu trong Modal (Create / Update)
public class TopicFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học.")]
    public int SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên chủ đề.")]
    [StringLength(200, ErrorMessage = "Tên chủ đề không được quá 200 ký tự.")]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thứ tự hiển thị.")]
    public int DisplayOrder { get; set; }

    [Required]
    public string Status { get; set; } = "Active";
}

// Class dùng để bọc dữ liệu phân trang trả về View Index giống bên Subjects
public class TopicListViewModel
{
    public List<TopicItemViewModel> Items { get; set; } = new();
    public long? Count { get; set; }
    public string? Keyword { get; set; }
    public int? SubjectId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}