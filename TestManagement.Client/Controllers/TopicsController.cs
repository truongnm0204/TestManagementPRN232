using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Topics;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize]
public class TopicsController : Controller
{
    private readonly TopicService _topicService;
    private readonly SubjectService _subjectService;

    public TopicsController(TopicService topicService, SubjectService subjectService)
    {
        _topicService = topicService;
        _subjectService = subjectService;
    }

    public async Task<IActionResult> Index(string? keyword, int? subjectId, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _topicService.GetListAsync(keyword, subjectId, status, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(new TopicListViewModel
        {
            Items = result.Data?.Items ?? new List<TopicItemViewModel>(),
            Count = result.Data?.Count,
            Subjects = await _subjectService.GetSubjectOptionsAsync(),
            Keyword = keyword,
            SubjectId = subjectId,
            Status = status,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _topicService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TopicFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu chủ đề không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _topicService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo chủ đề thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(TopicFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Dữ liệu chủ đề không hợp lệ.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _topicService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật chủ đề thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _topicService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa chủ đề thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
