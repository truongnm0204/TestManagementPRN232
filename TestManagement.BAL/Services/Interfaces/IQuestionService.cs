using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Questions;

namespace TestManagement.BAL.Services.Interfaces;

public interface IQuestionService
{
    IQueryable<QuestionODataResponse> GetODataQueryable();
    Task<QuestionResponse?> GetByIdAsync(int id);
    Task<ServiceResult<QuestionResponse>> CreateAsync(CreateQuestionRequest request, int? currentUserId);
    Task<ServiceResult> UpdateAsync(int id, UpdateQuestionRequest request);
    Task<ServiceResult> DeleteAsync(int id);
}
