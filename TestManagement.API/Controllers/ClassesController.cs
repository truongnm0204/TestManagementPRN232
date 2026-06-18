using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TestManagement.BAL.DTOs.Classes;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassService _classService;

    public ClassesController(IClassService classService)
    {
        _classService = classService;
    }

    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public IActionResult GetAll()
    {
        return Ok(_classService.GetODataQueryable());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var classResponse = await _classService.GetByIdAsync(id);
        if (classResponse == null) return NotFound("Không tìm thấy lớp học.");
        return Ok(classResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _classService.CreateAsync(request);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _classService.UpdateAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Cập nhật lớp học thành công.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _classService.DeleteAsync(id);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa lớp học thành công.");
    }

    [HttpPost("{id}/students")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> AddStudent(int id, [FromBody] AddStudentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _classService.AddStudentAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Thêm sinh viên vào lớp thành công.");
    }

    [HttpDelete("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin,Staff")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(id, studentId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa sinh viên khỏi lớp thành công.");
    }
}
