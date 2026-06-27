using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Classes;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize]
public class ClassesController : Controller
{
    private readonly ClassService _classService;
    private readonly UserService _userService;

    public ClassesController(ClassService classService, UserService userService)
    {
        _classService = classService;
        _userService = userService;
    }

    // UC21 — View Class List
    public async Task<IActionResult> Index(string? keyword, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _classService.GetListAsync(keyword, status, page, pageSize);
        if (!result.Success)
            TempData["Error"] = result.Error;

        return View(new ClassListViewModel
        {
            Items = result.Data?.Items ?? new List<ClassItemViewModel>(),
            Count = result.Data?.Count,
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = pageSize
        });
    }

    // UC25 + UC28 — View Class Detail & View Students in Class
    public async Task<IActionResult> Detail(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không tìm thấy lớp học.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    // UC22 — Create Class
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu lớp học không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _classService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo lớp học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    // UC23 — Update Class
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ClassFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu lớp học không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _classService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật lớp học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    // UC24 — Activate / Close Class
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, string status)
    {
        var result = await _classService.SetStatusAsync(id, status);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? $"Đã chuyển trạng thái lớp thành {status}."
            : result.Error;
        return RedirectToAction(nameof(Detail), new { id });
    }

    // UC26 — Add Student to Class
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(int classId, int studentId)
    {
        var result = await _classService.AddStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Thêm sinh viên vào lớp thành công." : result.Error;
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    // UC27 — Remove Student from Class
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(int classId, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa sinh viên khỏi lớp thành công." : result.Error;
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    // Helper: load danh sách students để chọn thêm vào lớp
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStudentOptions(string? keyword)
    {
        var result = await _userService.GetListAsync(keyword, "Student", true, 1, 20);
        if (!result.Success || result.Data == null)
            return Json(new List<object>());

        var options = result.Data.Items.Select(u => new { value = u.Id, text = $"{u.FullName} ({u.Email})" });
        return Json(options);
    }
}
