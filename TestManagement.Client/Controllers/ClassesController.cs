using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Classes;
using TestManagement.Client.Models.Common;
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

    public async Task<IActionResult> Index(string? keyword, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _classService.GetListAsync(keyword, status, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

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

    public async Task<IActionResult> Details(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không tìm thấy lớp học.";
            return RedirectToAction(nameof(Index));
        }

        return View(new ClassDetailViewModel
        {
            Class = result.Data,
            StudentOptions = await GetStudentOptionsAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
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

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _classService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa lớp học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(int classId, int studentId)
    {
        var result = await _classService.AddStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Thêm học viên vào lớp thành công." : result.Error;
        return RedirectToAction(nameof(Details), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(int classId, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa học viên khỏi lớp thành công." : result.Error;
        return RedirectToAction(nameof(Details), new { id = classId });
    }

    private async Task<List<SelectOptionViewModel>> GetStudentOptionsAsync()
    {
        var result = await _userService.GetListAsync(null, "Student", true, 1, 100);
        return result.Data?.Items.Select(student => new SelectOptionViewModel
        {
            Value = student.Id.ToString(),
            Text = $"{student.FullName} - {student.Email}"
        }).ToList() ?? new List<SelectOptionViewModel>();
    }
}
