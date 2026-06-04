using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TestManagement.BAL.DTOs.Questions;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public IActionResult GetAll()
    {
        return Ok(_questionService.GetODataQueryable());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var question = await _questionService.GetByIdAsync(id);

        if (question == null)
        {
            return NotFound("Không tìm thấy câu hỏi.");
        }

        return Ok(question);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateQuestionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _questionService.CreateAsync(request, GetCurrentUserId());

        if (!result.Success || result.Data == null)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQuestionRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _questionService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok("Cập nhật câu hỏi thành công.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _questionService.DeleteAsync(id);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok("Xóa câu hỏi thành công.");
    }

    private int? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdValue, out var userId) ? userId : null;
    }
}
