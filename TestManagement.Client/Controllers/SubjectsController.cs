using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Subjects;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize(Roles = "Admin,Staff,Teacher")]
public class SubjectsController : Controller
{
    private readonly SubjectService _subjectService;

    public SubjectsController(SubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    public async Task<IActionResult> Index(string? keyword, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _subjectService.GetListAsync(keyword, status, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(new SubjectListViewModel
        {
            Items = result.Data?.Items ?? new List<SubjectItemViewModel>(),
            Count = result.Data?.Count,
            Keyword = keyword,
            Status = status,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _subjectService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SubjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu môn học không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _subjectService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo môn học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(SubjectFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu môn học không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _subjectService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật môn học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subjectService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa môn học thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
