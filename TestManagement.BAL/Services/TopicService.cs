using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Topics;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class TopicService : ITopicService
{
    private static readonly string[] ValidStatuses = { "Active", "Inactive" };
    private readonly ITopicRepository _topicRepository;
    private readonly ISubjectRepository _subjectRepository;

    public TopicService(ITopicRepository topicRepository, ISubjectRepository subjectRepository)
    {
        _topicRepository = topicRepository;
        _subjectRepository = subjectRepository;
    }

    public IQueryable<TopicODataResponse> GetODataQueryable()
    {
        return _topicRepository.GetQueryable().Select(t => new TopicODataResponse
        {
            Id = t.Id,
            SubjectId = t.SubjectId,
            SubjectName = t.Subject != null ? t.Subject.Name : "",
            Name = t.Name,
            DisplayOrder = t.DisplayOrder,
            Status = t.Status,
            QuestionCount = t.Questions.Count(q => !q.IsDeleted),
            CreatedAt = t.CreatedAt
        });
    }

    public async Task<TopicResponse?> GetByIdAsync(int id)
    {
        var topic = await _topicRepository.GetActiveByIdAsync(id);
        return topic == null ? null : MapToResponse(topic);
    }

    public async Task<ServiceResult<TopicResponse>> CreateAsync(CreateTopicRequest request)
    {
        if (!IsValidStatus(request.Status))
            return ServiceResult<TopicResponse>.Fail("Trạng thái không hợp lệ.");

        var subject = await _subjectRepository.GetActiveByIdAsync(request.SubjectId);
        if (subject == null)
            return ServiceResult<TopicResponse>.Fail("Không tìm thấy môn học.");

        if (await _topicRepository.NameExistsInSubjectAsync(request.Name, request.SubjectId))
            return ServiceResult<TopicResponse>.Fail("Tên chủ đề đã tồn tại trong môn học này.");

        var topic = new Topic
        {
            SubjectId = request.SubjectId,
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            Status = request.Status,
            CreatedAt = DateTime.Now
        };

        await _topicRepository.AddAsync(topic);
        await _topicRepository.SaveChangesAsync();

        return ServiceResult<TopicResponse>.Ok(MapToResponse(topic));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateTopicRequest request)
    {
        if (!IsValidStatus(request.Status))
            return ServiceResult.Fail("Trạng thái không hợp lệ.");

        var topic = await _topicRepository.GetActiveByIdAsync(id);
        if (topic == null)
            return ServiceResult.Fail("Không tìm thấy chủ đề.");

        if (await _topicRepository.NameExistsInSubjectAsync(request.Name, topic.SubjectId, id))
            return ServiceResult.Fail("Tên chủ đề đã tồn tại trong môn học này.");

        topic.Name = request.Name;
        topic.Description = request.Description;
        topic.DisplayOrder = request.DisplayOrder;
        topic.Status = request.Status;
        topic.UpdatedAt = DateTime.Now;

        _topicRepository.Update(topic);
        await _topicRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var topic = await _topicRepository.GetActiveByIdAsync(id);
        if (topic == null)
            return ServiceResult.Fail("Không tìm thấy chủ đề.");

        topic.Status = "Inactive";
        topic.UpdatedAt = DateTime.Now;

        _topicRepository.Update(topic);
        await _topicRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<IEnumerable<TopicResponse>> GetBySubjectIdAsync(int subjectId)
    {
        var topics = await _topicRepository.GetBySubjectIdAsync(subjectId);
        return topics.Select(MapToResponse);
    }

    private static bool IsValidStatus(string status) => ValidStatuses.Contains(status);

    private static TopicResponse MapToResponse(Topic topic) => new()
    {
        Id = topic.Id,
        SubjectId = topic.SubjectId,
        SubjectName = topic.Subject?.Name ?? "",
        Name = topic.Name,
        Description = topic.Description,
        DisplayOrder = topic.DisplayOrder,
        Status = topic.Status,
        QuestionCount = topic.Questions?.Count(q => !q.IsDeleted) ?? 0,
        CreatedAt = topic.CreatedAt,
        UpdatedAt = topic.UpdatedAt
    };
}
