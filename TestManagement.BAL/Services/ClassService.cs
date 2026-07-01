using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Classes;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;
using Azure.Core;

namespace TestManagement.BAL.Services;

public class ClassService : IClassService
{
    private static readonly string[] ValidStatuses = { "Active", "Inactive", "Closed" };
    private readonly IClassRepository _classRepository;
    private readonly IUserRepository _userRepository;
   

    public ClassService(IClassRepository classRepository, IUserRepository userRepository)
    {
        _classRepository = classRepository;
        _userRepository = userRepository;
    }

    public IQueryable<ClassODataResponse> GetODataQueryable()
    {
        return _classRepository.GetQueryable().Select(c => new ClassODataResponse
        {
            Id = c.Id,
            ClassCode = c.ClassCode,
            ClassName = c.ClassName,
            Status = c.Status,
            StudentCount = c.StudentClasses.Count(sc => sc.Status == "Active"),
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<ClassResponse?> GetByIdAsync(int id)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(id);
        return classEntity == null ? null : MapToResponse(classEntity);
    }

    public async Task<ServiceResult<ClassResponse>> CreateAsync(CreateClassRequest request)
    {
        if (!IsValidStatus(request.Status))
            return ServiceResult<ClassResponse>.Fail("Trạng thái lớp học không hợp lệ.");

        if (await _classRepository.ClassCodeExistsAsync(request.ClassCode))
            return ServiceResult<ClassResponse>.Fail("Mã lớp học đã tồn tại.");

        var classEntity = new Class
        {
            ClassCode = request.ClassCode,
            ClassName = request.ClassName,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.Now
        };

        await _classRepository.AddAsync(classEntity);
        await _classRepository.SaveChangesAsync();

        return ServiceResult<ClassResponse>.Ok(MapToResponse(classEntity));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateClassRequest request)
    {
        if (!IsValidStatus(request.Status))
            return ServiceResult.Fail("Trạng thái lớp học không hợp lệ.");

        var classEntity = await _classRepository.GetByIdWithStudentsAsync(id);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        classEntity.ClassName = request.ClassName;
        classEntity.Description = request.Description;
        classEntity.Status = request.Status;
        classEntity.UpdatedAt = DateTime.Now;

        _classRepository.Update(classEntity);
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(id);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var activeStudents = classEntity.StudentClasses.Count(sc => sc.Status == "Active");
        if (activeStudents > 0)
            return ServiceResult.Fail("Không thể xóa lớp học đang có sinh viên.");

        _classRepository.Remove(classEntity);
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SetStatusAsync(int id, string status)
    {
        if (!IsValidStatus(status))
            return ServiceResult.Fail("Trạng thái không hợp lệ. Các giá trị hợp lệ: Active, Inactive, Closed.");

        var classEntity = await _classRepository.GetByIdWithStudentsAsync(id);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        classEntity.Status = status;
        classEntity.UpdatedAt = DateTime.Now;
        _classRepository.Update(classEntity);
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> AddStudentAsync(int classId, AddStudentRequest request)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var student = await _userRepository.GetActiveByIdAsync(request.StudentId);
        if (student == null || student.Role != "Student")
            return ServiceResult.Fail("Không tìm thấy sinh viên.");

        var existingStudentClass = await _classRepository.GetStudentClassAsync(request.StudentId, classId);
        if (existingStudentClass != null)
        {
            if (existingStudentClass.Status == "Active")
                return ServiceResult.Fail("Sinh viên đã có trong lớp học này.");

            existingStudentClass.Status = "Active";
            existingStudentClass.JoinedAt = DateTime.Now;
            await _classRepository.SaveChangesAsync();
            return ServiceResult.Ok();
        }

        var studentClass = new StudentClass
        {
            StudentId = request.StudentId,
            ClassId = classId,
            JoinedAt = DateTime.Now,
            Status = "Active"
        };

        classEntity.StudentClasses.Add(studentClass);
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveStudentAsync(int classId, int studentId)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var studentClass = classEntity.StudentClasses
            .FirstOrDefault(sc => sc.StudentId == studentId && sc.Status == "Active");

        if (studentClass == null)
            return ServiceResult.Fail("Sinh viên không có trong lớp học.");

        studentClass.Status = "Removed";
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private static bool IsValidStatus(string status) => ValidStatuses.Contains(status);

    private static ClassResponse MapToResponse(Class classEntity) => new()
    {
        Id = classEntity.Id,
        ClassCode = classEntity.ClassCode,
        ClassName = classEntity.ClassName,
        Description = classEntity.Description,
        Status = classEntity.Status,
        StudentCount = classEntity.StudentClasses?.Count(sc => sc.Status == "Active") ?? 0,
        CreatedAt = classEntity.CreatedAt,
        UpdatedAt = classEntity.UpdatedAt,
        Students = classEntity.StudentClasses?
            .Where(sc => sc.Status == "Active" && sc.Student != null)
            .Select(sc => new StudentInClassResponse
            {
                StudentId = sc.StudentId,
                FullName = sc.Student!.FullName,
                Email = sc.Student.Email,
                PhoneNumber = sc.Student.PhoneNumber,
                JoinedAt = sc.JoinedAt
            }).ToList() ?? new()
    };
}
