using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Questions;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize]
public class QuestionsController : Controller
{
    private readonly QuestionService _questionService;
    private readonly SubjectService _subjectService;

    public QuestionsController(QuestionService questionService, SubjectService subjectService)
    {
        _questionService = questionService;
        _subjectService = subjectService;
    }

    public async Task<IActionResult> Index(string? keyword, int? subjectId, string? difficulty, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _questionService.GetListAsync(keyword, subjectId, difficulty, status, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(new QuestionListViewModel
        {
            Items = result.Data?.Items ?? new List<QuestionItemViewModel>(),
            Subjects = await _subjectService.GetSubjectOptionsAsync(),
            Count = result.Data?.Count,
            Keyword = keyword,
            SubjectId = subjectId,
            Difficulty = difficulty,
            Status = status,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _questionService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionFormViewModel model)
    {
        if (!IsValidQuestion(model))
        {
            TempData["Error"] = "Dữ liệu câu hỏi không hợp lệ. Vui lòng nhập đủ 4 đáp án và chọn đúng 1 đáp án đúng.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _questionService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo câu hỏi thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(QuestionFormViewModel model)
    {
        if (!IsValidQuestion(model))
        {
            TempData["Error"] = "Dữ liệu câu hỏi không hợp lệ. Vui lòng nhập đủ 4 đáp án và chọn đúng 1 đáp án đúng.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _questionService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật câu hỏi thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _questionService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa câu hỏi thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private bool IsValidQuestion(QuestionFormViewModel model)
    {
        var validOptions = model.Options.Where(option => !string.IsNullOrWhiteSpace(option.Content)).ToList();
        return ModelState.IsValid
            && model.SubjectId > 0
            && model.Options.Count == 4
            && validOptions.Count == 4
            && model.CorrectOptionIndex >= 0
            && model.CorrectOptionIndex < model.Options.Count;
    }
}
