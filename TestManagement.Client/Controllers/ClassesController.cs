using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

    // ─── TEACHER: chỉ thấy lớp mình phụ trách ───────────────────────────────

    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Index(string? keyword, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _classService.GetMyClassesAsync(keyword, status, page, pageSize);
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

    [Authorize(Roles = "Teacher")]
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

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddStudent(int classId, int studentId)
    {
        var result = await _classService.AddStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Thêm sinh viên vào lớp thành công." : result.Error;
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveStudent(int classId, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa sinh viên khỏi lớp thành công." : result.Error;
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportStudents(int classId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(Detail), new { id = classId });
        }
        var result = await _classService.ImportStudentsAsync(classId, file);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            var r = result.Data!;
            TempData["Success"] = r.Summary;
            if (r.Errors.Any())
                TempData["Warning"] = "Email không hợp lệ: " + string.Join("; ", r.Errors.Take(5));
        }
        return RedirectToAction(nameof(Detail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LeaveClass(int classId)
    {
        var result = await _classService.LeaveClassAsync(classId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã rời khỏi lớp học." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DissolveClass(int classId)
    {
        var result = await _classService.DissolveClassAsync(classId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã giải tán lớp học." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> GetStudentOptions(string? keyword)
    {
        var result = await _userService.GetListAsync(keyword, "Student", true, 1, 20);
        if (!result.Success || result.Data == null)
            return Json(new List<object>());
        var options = result.Data.Items.Select(u => new { value = u.Id, text = $"{u.FullName} ({u.Email})" });
        return Json(options);
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> TeacherGetForEdit(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TeacherCreate(ClassFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu lớp học không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }
        model.Status = "Active";
        var result = await _classService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo lớp học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TeacherUpdate(ClassFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu lớp học không hợp lệ.";
            return RedirectToAction(nameof(Detail), new { id = model.Id });
        }
        var result = await _classService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật lớp học thành công." : result.Error;
        return RedirectToAction(nameof(Detail), new { id = model.Id });
    }

    // ─── ADMIN: quản lý tất cả lớp + phân công giáo viên ─────────────────────

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminIndex(string? keyword, string? status, int page = 1, int pageSize = 10)
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

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminDetail(int id)
    {
        var result = await _classService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không tìm thấy lớp học.";
            return RedirectToAction(nameof(AdminIndex));
        }
        return View(result.Data);
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
            return RedirectToAction(nameof(AdminIndex));
        }
        var result = await _classService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo lớp học thành công." : result.Error;
        return RedirectToAction(nameof(AdminIndex));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ClassFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu lớp học không hợp lệ.";
            return RedirectToAction(nameof(AdminIndex));
        }
        var result = await _classService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật lớp học thành công." : result.Error;
        return RedirectToAction(nameof(AdminIndex));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _classService.SetStatusAsync(id, status);
        TempData[result.Success ? "Success" : "Error"] = result.Success
            ? $"Đã chuyển trạng thái lớp thành {status}."
            : result.Error;
        return RedirectToAction(nameof(AdminDetail), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminImportStudents(int classId, IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Vui lòng chọn file Excel.";
            return RedirectToAction(nameof(AdminDetail), new { id = classId });
        }
        var result = await _classService.ImportStudentsAsync(classId, file);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }
        else
        {
            var r = result.Data!;
            TempData["Success"] = r.Summary;
            if (r.Errors.Any())
                TempData["Warning"] = "Email không hợp lệ: " + string.Join("; ", r.Errors.Take(5));
        }
        return RedirectToAction(nameof(AdminDetail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminAddStudent(int classId, int studentId)
    {
        var result = await _classService.AddStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Thêm sinh viên vào lớp thành công." : result.Error;
        return RedirectToAction(nameof(AdminDetail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AdminRemoveStudent(int classId, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(classId, studentId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa sinh viên khỏi lớp thành công." : result.Error;
        return RedirectToAction(nameof(AdminDetail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTeacher(int classId, int teacherId)
    {
        var result = await _classService.AddTeacherAsync(classId, teacherId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Phân công giáo viên thành công." : result.Error;
        return RedirectToAction(nameof(AdminDetail), new { id = classId });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveTeacher(int classId, int teacherId)
    {
        var result = await _classService.RemoveTeacherAsync(classId, teacherId);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Đã xóa giáo viên khỏi lớp." : result.Error;
        return RedirectToAction(nameof(AdminDetail), new { id = classId });
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetStudentOptionsAdmin(string? keyword)
    {
        var result = await _userService.GetListAsync(keyword, "Student", true, 1, 20);
        if (!result.Success || result.Data == null)
            return Json(new List<object>());
        var options = result.Data.Items.Select(u => new { value = u.Id, text = $"{u.FullName} ({u.Email})" });
        return Json(options);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTeacherOptions(string? keyword)
    {
        var result = await _userService.GetListAsync(keyword, "Teacher", true, 1, 20);
        if (!result.Success || result.Data == null)
            return Json(new List<object>());
        var options = result.Data.Items.Select(u => new { value = u.Id, text = $"{u.FullName} ({u.Email})" });
        return Json(options);
    }
}
