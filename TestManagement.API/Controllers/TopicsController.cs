using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TestManagement.BAL.DTOs.Topics;
using TestManagement.BAL.Services.Interfaces;

namespace TestManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    public IActionResult GetAll()
    {
        return Ok(_topicService.GetODataQueryable());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var topic = await _topicService.GetByIdAsync(id);
        if (topic == null) return NotFound("Không tìm thấy chủ đề.");
        return Ok(topic);
    }

    [HttpGet("by-subject/{subjectId}")]
    public async Task<IActionResult> GetBySubject(int subjectId)
    {
        var topics = await _topicService.GetBySubjectIdAsync(subjectId);
        return Ok(topics);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    public async Task<IActionResult> Create([FromBody] CreateTopicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _topicService.CreateAsync(request);
        if (!result.Success || result.Data == null) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Data.Id }, result.Data);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTopicRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var result = await _topicService.UpdateAsync(id, request);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Cập nhật chủ đề thành công.");
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Staff,Teacher")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _topicService.DeleteAsync(id);
        if (!result.Success) return BadRequest(result.Error);
        return Ok("Xóa chủ đề thành công.");
    }
}
