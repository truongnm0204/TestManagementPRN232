using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.Exams;
using TestManagement.Client.Models.Questions;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize(Roles = "Admin,Staff")]
public class ExamsController : Controller
{
    private readonly ExamService _examService;
    private readonly QuestionService _questionService;
    private readonly SubjectService _subjectService;

    public ExamsController(ExamService examService, QuestionService questionService, SubjectService subjectService)
    {
        _examService = examService;
        _questionService = questionService;
        _subjectService = subjectService;
    }

    public async Task<IActionResult> Index(string? keyword, int? subjectId, string? status, bool? isPublished, int page = 1, int pageSize = 10)
    {
        var result = await _examService.GetListAsync(keyword, subjectId, status, isPublished, page, pageSize);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(new ExamListViewModel
        {
            Items = result.Data?.Items ?? new List<ExamItemViewModel>(),
            Subjects = await _subjectService.GetSubjectOptionsAsync(),
            Count = result.Data?.Count,
            Keyword = keyword,
            SubjectId = subjectId,
            Status = status,
            IsPublished = isPublished,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create()
    {
        return View("Form", new ExamFormPageViewModel
        {
            Subjects = await _subjectService.GetSubjectOptionsAsync(),
            IsEdit = false
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Form")] ExamFormViewModel model)
    {
        NormalizeExamForm(model);
        if (!IsValidExam(model))
        {
            TempData["Error"] = "Dữ liệu đề thi không hợp lệ. Vui lòng kiểm tra môn học, tiêu đề, thời gian và lịch mở đề.";
            return View("Form", new ExamFormPageViewModel
            {
                Form = model,
                Subjects = await _subjectService.GetSubjectOptionsAsync(),
                IsEdit = false
            });
        }

        var result = await _examService.CreateAsync(model);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error;
            return View("Form", new ExamFormPageViewModel
            {
                Form = model,
                Subjects = await _subjectService.GetSubjectOptionsAsync(),
                IsEdit = false
            });
        }

        TempData["Success"] = "Tạo đề thi thành công. Hãy chọn câu hỏi cho đề thi.";
        return RedirectToAction(nameof(Compose), new { id = result.Data.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _examService.GetByIdAsync(id);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không tìm thấy đề thi.";
            return RedirectToAction(nameof(Index));
        }

        if (!result.Data.CanEdit)
        {
            TempData["Error"] = "Chỉ được cập nhật đề thi ở trạng thái Draft.";
            return RedirectToAction(nameof(Compose), new { id });
        }

        return View("Form", new ExamFormPageViewModel
        {
            Form = MapToForm(result.Data),
            Subjects = await _subjectService.GetSubjectOptionsAsync(),
            IsEdit = true
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Form")] ExamFormViewModel model)
    {
        NormalizeExamForm(model);
        if (!IsValidExam(model))
        {
            TempData["Error"] = "Dữ liệu đề thi không hợp lệ. Vui lòng kiểm tra môn học, tiêu đề, thời gian và lịch mở đề.";
            return View("Form", new ExamFormPageViewModel
            {
                Form = model,
                Subjects = await _subjectService.GetSubjectOptionsAsync(),
                IsEdit = true
            });
        }

        var result = await _examService.UpdateAsync(model);
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
            return View("Form", new ExamFormPageViewModel
            {
                Form = model,
                Subjects = await _subjectService.GetSubjectOptionsAsync(),
                IsEdit = true
            });
        }

        TempData["Success"] = "Cập nhật đề thi thành công.";
        return RedirectToAction(nameof(Compose), new { id = model.Id });
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Compose(int id)
    {
        var examResult = await _examService.GetByIdAsync(id);
        if (!examResult.Success || examResult.Data == null)
        {
            TempData["Error"] = examResult.Error ?? "Không tìm thấy đề thi.";
            return RedirectToAction(nameof(Index));
        }

        var selectedResult = await _examService.GetQuestionsAsync(id);
        if (!selectedResult.Success)
        {
            TempData["Error"] = selectedResult.Error;
        }

        var candidateResult = await _questionService.GetListAsync(null, examResult.Data.SubjectId, null, "Active", 1, 100);
        if (!candidateResult.Success)
        {
            TempData["Error"] = candidateResult.Error;
        }

        var selectedQuestions = selectedResult.Data ?? new ExamQuestionsViewModel { ExamId = id };
        var selectedMap = selectedQuestions.Items.ToDictionary(x => x.QuestionId);
        var candidates = (candidateResult.Data?.Items ?? new List<QuestionItemViewModel>())
            .Select((question, index) =>
            {
                selectedMap.TryGetValue(question.Id, out var selected);
                return new ExamQuestionSelectionViewModel
                {
                    QuestionId = question.Id,
                    Content = question.Content,
                    Difficulty = question.Difficulty,
                    OptionCount = question.Options.Count,
                    IsSelected = selected != null,
                    Points = selected?.Points ?? 1,
                    SortOrder = selected?.SortOrder ?? index + 1
                };
            })
            .OrderByDescending(x => x.IsSelected)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.QuestionId)
            .ToList();

        var candidateIds = candidates.Select(x => x.QuestionId).ToHashSet();
        var missingSelectedQuestions = selectedQuestions.Items
            .Where(x => !candidateIds.Contains(x.QuestionId))
            .Select(x => new ExamQuestionSelectionViewModel
            {
                QuestionId = x.QuestionId,
                Content = x.Content,
                Difficulty = x.Difficulty,
                OptionCount = x.Options.Count,
                IsSelected = true,
                Points = x.Points,
                SortOrder = x.SortOrder
            });
        candidates.AddRange(missingSelectedQuestions);
        candidates = candidates
            .OrderByDescending(x => x.IsSelected)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.QuestionId)
            .ToList();

        return View(new ExamComposeViewModel
        {
            Exam = examResult.Data,
            SelectedQuestions = selectedQuestions,
            CandidateQuestions = candidates
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuestions(int id, ExamComposePostViewModel model)
    {
        var selectedItems = BuildSelectedQuestionItems(model);

        if (!selectedItems.Any())
        {
            TempData["Error"] = "Đề thi phải có ít nhất một câu hỏi.";
            return RedirectToAction(nameof(Compose), new { id });
        }

        var result = await _examService.UpdateQuestionsAsync(id, new UpdateExamQuestionsViewModel { Items = selectedItems });
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Cập nhật câu hỏi cho đề thi thành công." : result.Error;
        return RedirectToAction(nameof(Compose), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, ExamComposePostViewModel model)
    {
        if (model.Questions.Any())
        {
            var selectedItems = BuildSelectedQuestionItems(model);
            if (!selectedItems.Any())
            {
                TempData["Error"] = "Đề thi phải có ít nhất một câu hỏi trước khi publish.";
                return RedirectToAction(nameof(Compose), new { id });
            }

            var updateResult = await _examService.UpdateQuestionsAsync(id, new UpdateExamQuestionsViewModel { Items = selectedItems });
            if (!updateResult.Success)
            {
                TempData["Error"] = updateResult.Error;
                return RedirectToAction(nameof(Compose), new { id });
            }
        }

        var result = await _examService.PublishAsync(id);
        if (result.Success && result.Data != null)
        {
            TempData["Success"] = $"Publish đề thi thành công: {result.Data.QuestionCount} câu, tổng {result.Data.TotalPoints} điểm.";
        }
        else
        {
            TempData["Error"] = result.Error ?? "Publish đề thi không thành công.";
        }
        return RedirectToAction(nameof(Compose), new { id });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _examService.DeleteAsync(id);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Xóa đề thi thành công." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private static void NormalizeExamForm(ExamFormViewModel model)
    {
        model.Title = model.Title?.Trim() ?? string.Empty;
        model.Description = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
    }

    private static List<ExamQuestionItemFormViewModel> BuildSelectedQuestionItems(ExamComposePostViewModel model)
    {
        return model.Questions
            .Where(x => x.IsSelected)
            .Select((question, index) => new ExamQuestionItemFormViewModel
            {
                QuestionId = question.QuestionId,
                Points = question.Points <= 0 ? 1 : question.Points,
                SortOrder = question.SortOrder <= 0 ? index + 1 : question.SortOrder
            })
            .ToList();
    }

    private static ExamFormViewModel MapToForm(ExamItemViewModel exam)
    {
        return new ExamFormViewModel
        {
            Id = exam.Id,
            SubjectId = exam.SubjectId,
            Title = exam.Title,
            Description = exam.Description,
            DurationMinutes = exam.DurationMinutes,
            AvailableFrom = exam.AvailableFrom,
            AvailableTo = exam.AvailableTo,
            AttemptLimit = exam.AttemptLimit,
            ShuffleQuestions = exam.ShuffleQuestions,
            ShuffleOptions = exam.ShuffleOptions,
            ShowResultsImmediately = exam.ShowResultsImmediately,
            ShowCorrectAnswers = exam.ShowCorrectAnswers
        };
    }

    private bool IsValidExam(ExamFormViewModel model)
    {
        return ModelState.IsValid
            && !string.IsNullOrWhiteSpace(model.Title)
            && model.SubjectId > 0
            && model.DurationMinutes > 0
            && model.AttemptLimit > 0
            && (!model.AvailableFrom.HasValue || !model.AvailableTo.HasValue || model.AvailableFrom < model.AvailableTo);
    }
}
