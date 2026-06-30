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
    Task<ServiceResult> SetStatusAsync(int id, string status);
    Task<ServiceResult> AddStudentAsync(int classId, AddStudentRequest request);
    Task<ServiceResult> RemoveStudentAsync(int classId, int studentId);
    Task<ServiceResult> AddTeacherAsync(int classId, AddTeacherRequest request);
    Task<ServiceResult> RemoveTeacherAsync(int classId, int teacherId);
    IQueryable<ClassODataResponse> GetODataQueryableForTeacher(int teacherId);
    Task<bool> IsTeacherOfClassAsync(int teacherId, int classId);
    Task<ServiceResult> LeaveClassAsync(int classId, int teacherId);
    Task<ServiceResult> DissolveClassAsync(int classId, int teacherId);
    Task<ServiceResult<ImportStudentsResult>> ImportStudentsFromExcelAsync(int classId, Stream excelStream);
}
