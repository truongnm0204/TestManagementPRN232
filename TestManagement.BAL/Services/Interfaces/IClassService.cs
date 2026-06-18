using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Classes;

namespace TestManagement.BAL.Services.Interfaces;

public interface IClassService
{
    IQueryable<ClassODataResponse> GetODataQueryable();
    Task<ClassResponse?> GetByIdAsync(int id);
    Task<ServiceResult<ClassResponse>> CreateAsync(CreateClassRequest request);
    Task<ServiceResult> UpdateAsync(int id, UpdateClassRequest request);
    Task<ServiceResult> DeleteAsync(int id);
    Task<ServiceResult> AddStudentAsync(int classId, AddStudentRequest request);
    Task<ServiceResult> RemoveStudentAsync(int classId, int studentId);
}
