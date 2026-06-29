using TestManagement.Client.Models.Common;

namespace TestManagement.Client.Models.Classes;

public class ClassDetailViewModel
{
    public ClassItemViewModel Class { get; set; } = new();
    public List<SelectOptionViewModel> StudentOptions { get; set; } = new();
}
