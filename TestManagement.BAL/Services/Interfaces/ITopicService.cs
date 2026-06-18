using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Topics;

namespace TestManagement.BAL.Services.Interfaces;

public interface ITopicService
{
    IQueryable<TopicODataResponse> GetODataQueryable();
    Task<TopicResponse?> GetByIdAsync(int id);
    Task<ServiceResult<TopicResponse>> CreateAsync(CreateTopicRequest request);
    Task<ServiceResult> UpdateAsync(int id, UpdateTopicRequest request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<IEnumerable<TopicResponse>> GetBySubjectIdAsync(int subjectId);
}
