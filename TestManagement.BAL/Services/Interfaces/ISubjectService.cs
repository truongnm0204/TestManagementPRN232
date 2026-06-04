using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Subjects;

namespace TestManagement.BAL.Services.Interfaces;

public interface ISubjectService
{
    IQueryable<SubjectODataResponse> GetODataQueryable();
    Task<SubjectResponse?> GetByIdAsync(int id);
    Task<ServiceResult<SubjectResponse>> CreateAsync(CreateSubjectRequest request);
    Task<ServiceResult> UpdateAsync(int id, UpdateSubjectRequest request);
    Task<ServiceResult> DeleteAsync(int id);
}
