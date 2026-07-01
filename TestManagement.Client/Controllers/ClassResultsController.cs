using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.Client.Models.ExamResults;
using TestManagement.Client.Services;

namespace TestManagement.Client.Controllers;

[Authorize(Roles = "Teacher,Admin")]
public class ClassResultsController : Controller
{
    private readonly ExamResultService _resultService;

    public ClassResultsController(ExamResultService resultService)
    {
        _resultService = resultService;
    }

    // GET /ClassResults — danh sách cặp (đề × lớp) đã giao
    public async Task<IActionResult> Index()
    {
        var result = await _resultService.GetAssignmentsAsync();
        if (!result.Success)
        {
            TempData["Error"] = result.Error;
        }

        return View(result.Data ?? new List<ExamAssignmentSummaryViewModel>());
    }

    // GET /ClassResults/ClassResult?examId={}&classId={} — bảng điểm cả lớp
    [HttpGet]
    public async Task<IActionResult> ClassResult(int examId, int classId)
    {
        var result = await _resultService.GetClassResultAsync(examId, classId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể tải kết quả lớp.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    // GET /ClassResults/Monitor?examId={}&classId={} — theo dõi realtime
    [HttpGet]
    public async Task<IActionResult> Monitor(int examId, int classId)
    {
        var result = await _resultService.GetClassResultAsync(examId, classId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể mở trang theo dõi.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    // GET /ClassResults/History/{attemptId} — lịch sử đổi đáp án của 1 student
    [HttpGet]
    public async Task<IActionResult> History(int attemptId)
    {
        var result = await _resultService.GetAttemptHistoryAsync(attemptId);
        if (!result.Success || result.Data == null)
        {
            TempData["Error"] = result.Error ?? "Không thể tải lịch sử.";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }
}
