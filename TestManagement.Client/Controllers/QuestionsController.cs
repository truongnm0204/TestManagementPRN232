using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

    // Admin: full list with filters
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Index(string? keyword, int? subjectId, string? difficulty, string? status, int page = 1, int pageSize = 10)
    {
        var result = await _questionService.GetListAsync(keyword, subjectId, difficulty, status, page, pageSize);
        if (!result.Success)
            TempData["Error"] = result.Error;

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

    // Teacher: subject package list (all subjects + teacher's question counts)
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> Packages()
    {
        var teacherId = GetCurrentUserId();
        if (teacherId == null)
            return RedirectToAction("Login", "Auth");

        var allSubjects = await _subjectService.GetSubjectOptionsAsync();
        var myPackages = await _questionService.GetMySubjectPackagesAsync(teacherId.Value);
        var countBySubject = myPackages.ToDictionary(p => p.SubjectId, p => p.QuestionCount);

        var packages = allSubjects.Select(s => new SubjectPackageViewModel
        {
            SubjectId = int.Parse(s.Value),
            SubjectCode = s.Text.Split(" - ").FirstOrDefault() ?? s.Text,
            SubjectName = s.Text.Contains(" - ") ? s.Text[(s.Text.IndexOf(" - ") + 3)..] : s.Text,
            QuestionCount = countBySubject.TryGetValue(int.Parse(s.Value), out var c) ? c : 0
        }).ToList();

        return View(packages);
    }

    // Teacher: questions in one subject package
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> BySubject(int subjectId, string? keyword, string? difficulty, string? status, int page = 1, int pageSize = 10)
    {
        var teacherId = GetCurrentUserId();
        if (teacherId == null)
            return RedirectToAction("Login", "Auth");

        var result = await _questionService.GetListAsync(keyword, subjectId, difficulty, status, page, pageSize, createdByUserId: teacherId.Value);
        if (!result.Success)
            TempData["Error"] = result.Error;

        var subjects = await _subjectService.GetSubjectOptionsAsync();
        var subjectName = subjects.FirstOrDefault(s => s.Value == subjectId.ToString())?.Text ?? string.Empty;

        return View(new QuestionListViewModel
        {
            Items = result.Data?.Items ?? new List<QuestionItemViewModel>(),
            Subjects = subjects,
            Count = result.Data?.Count,
            Keyword = keyword,
            SubjectId = subjectId,
            SubjectName = subjectName,
            Difficulty = difficulty,
            Status = status,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> GetForEdit(int id)
    {
        var result = await _questionService.GetByIdAsync(id);
        return result.Success && result.Data != null ? Json(result.Data) : BadRequest(result.Error);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuestionFormViewModel model, int? returnSubjectId)
    {
        if (!IsValidQuestion(model))
        {
            TempData["Error"] = "Dữ liệu câu hỏi không hợp lệ. Vui lòng nhập đủ 4 đáp án và chọn đúng 1 đáp án đúng.";
            return User.IsInRole("Teacher")
                ? RedirectToAction(nameof(BySubject), new { subjectId = returnSubjectId ?? model.SubjectId })
                : RedirectToAction(nameof(Index));
        }

        var result = await _questionService.CreateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Tạo câu hỏi thành công." : result.Error;

        return User.IsInRole("Teacher")
            ? RedirectToAction(nameof(BySubject), new { subjectId = returnSubjectId ?? model.SubjectId })
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(QuestionFormViewModel model, int? returnSubjectId)
    {
        if (!IsValidQuestion(model))
        {
            TempData["Error"] = "Dữ liệu câu hỏi không hợp lệ. Vui lòng nhập đủ 4 đáp án và chọn đúng 1 đáp án đúng.";
            return User.IsInRole("Teacher")
                ? RedirectToAction(nameof(BySubject), new { subjectId = returnSubjectId ?? model.SubjectId })
                : RedirectToAction(nameof(Index));
        }

        var result = await _questionService.UpdateAsync(model);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật câu hỏi thành công." : result.Error;

        return User.IsInRole("Teacher")
            ? RedirectToAction(nameof(BySubject), new { subjectId = returnSubjectId ?? model.SubjectId })
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? returnSubjectId)
    {
        var result = await _questionService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa câu hỏi thành công." : result.Error;

        return User.IsInRole("Teacher") && returnSubjectId.HasValue
            ? RedirectToAction(nameof(BySubject), new { subjectId = returnSubjectId.Value })
            : RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }

    private bool IsValidQuestion(QuestionFormViewModel model)
    {
        var validOptions = model.Options.Where(o => !string.IsNullOrWhiteSpace(o.Content)).ToList();
        return ModelState.IsValid
            && model.SubjectId > 0
            && model.Options.Count == 4
            && validOptions.Count == 4
            && model.CorrectOptionIndex >= 0
            && model.CorrectOptionIndex < model.Options.Count;
    }
}
