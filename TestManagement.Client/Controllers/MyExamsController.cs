using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.ExamAttempts;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize(Roles = "Student")]
public class MyExamsController : Controller
{
    private readonly ExamAttemptService _attemptService;

    public MyExamsController(ExamAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    // GET /MyExams — danh sách đề thi được giao
    public async Task<IActionResult> Index()
    {
        var result = await _attemptService.GetMyExamsAsync();
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(result.Data ?? new List<MyExamItemViewModel>());
    }

    // POST /MyExams/Start — gọi API tạo/resume attempt rồi redirect sang Take
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int examId)
    {
        var result = await _attemptService.StartAsync(examId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể bắt đầu làm bài.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Take), new { attemptId = result.Data.AttemptId });
    }

    // GET /MyExams/Take/{attemptId} — màn làm bài (gọi API nên refresh được)
    [HttpGet]
    public async Task<IActionResult> Take(int attemptId)
    {
        var result = await _attemptService.GetAttemptAsync(attemptId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể mở bài làm.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    // POST /MyExams/SaveAnswer — auto-save 1 câu, trả JSON (gọi qua AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveAnswer(int attemptId, [FromForm] SaveAnswerRequestModel model)
    {
        var result = await _attemptService.SaveAnswerAsync(attemptId, model);
        if (!result.Success)
        {
            return Json(new { success = false, error = result.Error });
        }

        return Json(new { success = true });
    }

    // POST /MyExams/Submit — nộp bài rồi sang Result
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int attemptId)
    {
        var result = await _attemptService.SubmitAsync(attemptId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể nộp bài.";
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Result), new { attemptId });
    }

    // GET /MyExams/Result/{attemptId} — xem kết quả
    [HttpGet]
    public async Task<IActionResult> Result(int attemptId)
    {
        var result = await _attemptService.GetResultAsync(attemptId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không tìm thấy kết quả.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}
