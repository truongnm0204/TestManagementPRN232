using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TestManagement.BAL.DTOs.Exams;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ExamsController : ControllerBase
{
    private readonly IExamService _examService;

    public ExamsController(IExamService examService)
    {
        _examService = examService;
    }

    // GET api/exams — OData: hỗ trợ $filter, $orderby, $top, $skip, $count
    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public IActionResult GetAll()
    {
        return Ok(_examService.GetOdataQueryable());
    }

    // GET api/exams/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var exam = await _examService.GetByIdAsync(id);
        if (exam == null) return NotFound("Không tìm thấy đề thi.");
        return Ok(exam);
    }

    // POST api/exams — chỉ Admin/Staff được tạo
    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateExamRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _examService.CreateAsync(request, GetCurrentUserId());
        if (!result.Success || result.Data == null) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    // PUT api/exams/{id}
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExamRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _examService.UpdateAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);

        return Ok("Cập nhật đề thi thành công.");
    }

    // DELETE api/exams/{id} — soft delete, chỉ Admin
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _examService.DeleteAsync(id);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa đề thi thành công.");
    }

    // đọc UserId từ JWT claim "NameIdentifier" — JwtBearer đã set sẵn ở AuthService
    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
