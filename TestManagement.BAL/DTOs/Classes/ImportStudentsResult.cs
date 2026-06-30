namespace TestManagement.BAL.DTOs.Classes;

public class ImportStudentsResult
{
    public int Added { get; set; }
    public int NewUsersCreated { get; set; }
    public int AlreadyInClass { get; set; }
    public int InvalidRows { get; set; }
    public List<string> Errors { get; set; } = new();

    public string Summary =>
        $"Thêm vào lớp: {Added} (tạo mới tài khoản: {NewUsersCreated}), Đã có trong lớp: {AlreadyInClass}, Lỗi: {InvalidRows}";
}
