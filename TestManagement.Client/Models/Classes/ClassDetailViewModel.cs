using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Models.Classes;

public class ClassDetailViewModel
{
    public ClassItemViewModel Class { get; set; } = new();
    public List<SelectOptionViewModel> StudentOptions { get; set; } = new();
}
public class TeacherInClassViewModel
{
    public int TeacherId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime AssignedAt { get; set; }
}
