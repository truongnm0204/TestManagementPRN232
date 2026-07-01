using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.BAL.DTOs.ExamAttempts;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/exam-attempts")]
[ApiController]
[Authorize(Roles = "Student")]
public class ExamAttemptsController : ControllerBase
{
    private readonly IExamAttemptService _attemptService;

    public ExamAttemptsController(IExamAttemptService attemptService)
    {
        _attemptService = attemptService;
    }

    // GET api/exam-attempts/my-exams — danh sách đề thi của student
    [HttpGet("my-exams")]
    public async Task<IActionResult> GetMyExams()
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.GetMyExamsAsync(studentId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // POST api/exam-attempts — bắt đầu / resume làm bài
    [HttpPost]
    public async Task<IActionResult> Start([FromBody] StartAttemptRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.StartAsync(request.ExamId, studentId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // GET api/exam-attempts/{id} — lấy lại đề của attempt đang làm (resume khi refresh)
    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttempt(int id)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.GetAttemptAsync(id, studentId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // POST api/exam-attempts/{id}/answers — lưu đáp án một câu (auto-save)
    [HttpPost("{id}/answers")]
    public async Task<IActionResult> SaveAnswer(int id, [FromBody] SaveAnswerRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.SaveAnswerAsync(id, request, studentId.Value);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Đã lưu đáp án.");
    }

    // POST api/exam-attempts/{id}/submit — nộp bài
    [HttpPost("{id}/submit")]
    public async Task<IActionResult> Submit(int id)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.SubmitAsync(id, studentId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // GET api/exam-attempts/{id}/result — xem kết quả
    [HttpGet("{id}/result")]
    public async Task<IActionResult> GetResult(int id)
    {
        var studentId = GetCurrentUserId();
        if (studentId == null) return Unauthorized();

        var result = await _attemptService.GetResultAsync(id, studentId.Value);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    private int? GetCurrentUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}

// Request DTO nhỏ chỉ dùng ở Controller
public class StartAttemptRequest
{
    public int ExamId { get; set; }
}
