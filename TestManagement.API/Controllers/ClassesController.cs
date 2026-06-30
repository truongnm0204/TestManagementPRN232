using System.Security.Claims;
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

    [HttpGet("my")]
    [EnableQuery(MaxTop = 100)]
    [Authorize(Roles = "Teacher")]
    public IActionResult GetMyClasses()
    {
        var teacherIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(teacherIdClaim, out var teacherId))
            return Unauthorized();
        return Ok(_classService.GetODataQueryableForTeacher(teacherId));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var classResponse = await _classService.GetByIdAsync(id);
        if (classResponse == null) return NotFound("Không tìm thấy lớp học.");
        return Ok(classResponse);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateClassRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Giáo viên tạo lớp → ghi nhận người tạo
        if (User.IsInRole("Teacher"))
        {
            var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(teacherIdStr, out var tid))
                request.CreatedBy = tid;
        }

        var result = await _classService.CreateAsync(request);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);

        // Giáo viên tạo lớp → tự động phân công phụ trách
        if (User.IsInRole("Teacher") && request.CreatedBy.HasValue)
            await _classService.AddTeacherAsync(result.Data.Id, new AddTeacherRequest { TeacherId = request.CreatedBy.Value });

        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClassRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Giáo viên chỉ được sửa lớp mình phụ trách
        if (User.IsInRole("Teacher"))
        {
            var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(teacherIdStr, out var teacherId) || !await _classService.IsTeacherOfClassAsync(teacherId, id))
                return Forbid();
        }

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

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] string status)
    {
        var result = await _classService.SetStatusAsync(id, status);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Cập nhật trạng thái lớp học thành công.");
    }

    [HttpPost("{id}/students")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> AddStudent(int id, [FromBody] AddStudentRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _classService.AddStudentAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Thêm sinh viên vào lớp thành công.");
    }

    [HttpDelete("{id}/students/{studentId}")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> RemoveStudent(int id, int studentId)
    {
        var result = await _classService.RemoveStudentAsync(id, studentId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa sinh viên khỏi lớp thành công.");
    }

    [HttpPost("{id}/leave")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> LeaveClass(int id)
    {
        var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(teacherIdStr, out var teacherId)) return Unauthorized();
        var result = await _classService.LeaveClassAsync(id, teacherId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Đã rời khỏi lớp học.");
    }

    [HttpDelete("{id}/dissolve")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DissolveClass(int id)
    {
        var teacherIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(teacherIdStr, out var teacherId)) return Unauthorized();
        var result = await _classService.DissolveClassAsync(id, teacherId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Đã giải tán lớp học.");
    }

    [HttpPost("{id}/students/import")]
    [Authorize(Roles = "Admin,Teacher")]
    public async Task<IActionResult> ImportStudents(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Vui lòng chọn file Excel.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xls")
            return BadRequest("Chỉ hỗ trợ file Excel (.xlsx, .xls).");

        using var stream = file.OpenReadStream();
        var result = await _classService.ImportStudentsFromExcelAsync(id, stream);
        if (!result.Success) return BadRequest(result.Error);
        return Ok(result.Data);
    }

    [HttpPost("{id}/teachers")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddTeacher(int id, [FromBody] AddTeacherRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _classService.AddTeacherAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Phân công giáo viên vào lớp thành công.");
    }

    [HttpDelete("{id}/teachers/{teacherId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RemoveTeacher(int id, int teacherId)
    {
        var result = await _classService.RemoveTeacherAsync(id, teacherId);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa giáo viên khỏi lớp thành công.");
    }
}
