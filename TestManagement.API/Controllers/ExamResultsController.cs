using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/exam-results")]
[ApiController]
[Authorize(Roles = "Teacher,Admin")]
public class ExamResultsController : ControllerBase
{
    private readonly IExamResultService _resultService;

    public ExamResultsController(IExamResultService resultService)
    {
        _resultService = resultService;
    }

    // GET api/exam-results/assignments — các cặp (đề × lớp) đã giao
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments()
    {
        var result = await _resultService.GetTeacherAssignmentsAsync();
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // GET api/exam-results/class?examId={}&classId={} — kết quả của cả lớp
    [HttpGet("class")]
    public async Task<IActionResult> GetClassResult([FromQuery] int examId, [FromQuery] int classId)
    {
        var result = await _resultService.GetClassResultAsync(examId, classId);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    // GET api/exam-results/attempts/{attemptId}/history — lịch sử đổi đáp án
    [HttpGet("attempts/{attemptId}/history")]
    public async Task<IActionResult> GetAttemptHistory(int attemptId)
    {
        var result = await _resultService.GetAttemptHistoryAsync(attemptId);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return Ok(result.Data);
    }
}
