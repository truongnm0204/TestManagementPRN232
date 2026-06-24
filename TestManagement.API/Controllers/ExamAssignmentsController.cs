using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using System.Threading.Tasks;
using TestManagement.BAL.DTOs.ExamAssignments;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExamAssignmentsController : ControllerBase
    {
        private readonly IExamAssignmentService _assignmentService;

        public ExamAssignmentsController(IExamAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet]
        [EnableQuery(MaxTop = 100)]
        public IActionResult GetAll()
        {
            return Ok(_assignmentService.GetODataQueryable());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await _assignmentService.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound("Không tìm thấy thông tin phân bổ bài thi.");
            }
            return Ok(assignment);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateAssignmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _assignmentService.CreateAsync(request, GetCurrentUserId());
            if (!result.Success || result.Data == null)
            {
                return BadRequest(result.Error);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _assignmentService.DeleteAsync(id);
            if (!result.Success)
            {
                return BadRequest(result.Error);
            }
            return Ok("Hủy phân bổ bài thi thành công.");
        }

        private int? GetCurrentUserId()
        {
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameIdentifier, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}