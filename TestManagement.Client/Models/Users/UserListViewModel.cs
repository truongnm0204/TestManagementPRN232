namespace TestManagement.Client.Models.Users;

public class UserListViewModel
{
    public List<UserItemViewModel> Items { get; set; } = new();
    public string? Keyword { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int? Count { get; set; }

    public bool HasPrevious => Page > 1;
    public bool HasNext => Count.HasValue ? Page * PageSize < Count.Value : Items.Count == PageSize;
}
