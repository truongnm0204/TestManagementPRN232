using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TestManagement.BAL.DTOs.Subjects;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;

    public SubjectsController(ISubjectService subjectService)
    {
        _subjectService = subjectService;
    }

    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public IActionResult GetAll()
    {
        return Ok(_subjectService.GetODataQueryable());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subject = await _subjectService.GetByIdAsync(id);

        if (subject == null)
        {
            return NotFound("Không tìm thấy môn học.");
        }

        return Ok(subject);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateSubjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _subjectService.CreateAsync(request);

        if (!result.Success || result.Data == null)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSubjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _subjectService.UpdateAsync(id, request);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok("Cập nhật môn học thành công.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _subjectService.DeleteAsync(id);

        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return Ok("Xóa môn học thành công.");
    }
}
