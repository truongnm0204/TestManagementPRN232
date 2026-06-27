namespace TestManagement.Client.Models.Classes;

public class ClassListViewModel
{
    public List<ClassItemViewModel> Items { get; set; } = new();
    public string? Keyword { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Count { get; set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Count.HasValue ? Page * PageSize < Count.Value : Items.Count == PageSize;
}
