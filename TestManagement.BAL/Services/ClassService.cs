using BCrypt.Net;
using ClosedXML.Excel;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Classes;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

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
            CreatedBy = c.CreatedBy,
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
            CreatedBy = request.CreatedBy,
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

    public async Task<ServiceResult> AddTeacherAsync(int classId, AddTeacherRequest request)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var teacher = await _userRepository.GetActiveByIdAsync(request.TeacherId);
        if (teacher == null || teacher.Role != "Teacher")
            return ServiceResult.Fail("Không tìm thấy giáo viên.");

        // Xóa giáo viên cũ nếu có (mỗi lớp chỉ có 1 giáo viên)
        var existing = classEntity.TeacherClasses.FirstOrDefault();
        if (existing != null)
        {
            if (existing.TeacherId == request.TeacherId)
                return ServiceResult.Fail("Giáo viên này đã được phân công vào lớp học.");
            classEntity.TeacherClasses.Remove(existing);
        }

        classEntity.TeacherClasses.Add(new TeacherClass
        {
            TeacherId = request.TeacherId,
            ClassId = classId,
            AssignedAt = DateTime.Now
        });
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RemoveTeacherAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var teacherClass = classEntity.TeacherClasses.FirstOrDefault(tc => tc.TeacherId == teacherId);
        if (teacherClass == null)
            return ServiceResult.Fail("Giáo viên không thuộc lớp học này.");

        classEntity.TeacherClasses.Remove(teacherClass);
        await _classRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public Task<bool> IsTeacherOfClassAsync(int teacherId, int classId)
        => _classRepository.IsTeacherInClassAsync(teacherId, classId);

    public IQueryable<ClassODataResponse> GetODataQueryableForTeacher(int teacherId)
    {
        return _classRepository.GetQueryableForTeacher(teacherId).Select(c => new ClassODataResponse
        {
            Id = c.Id,
            ClassCode = c.ClassCode,
            ClassName = c.ClassName,
            Status = c.Status,
            StudentCount = c.StudentClasses.Count(sc => sc.Status == "Active"),
            CreatedBy = c.CreatedBy,
            CreatedAt = c.CreatedAt
        });
    }

    public async Task<ServiceResult> LeaveClassAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        var teacherClass = classEntity.TeacherClasses.FirstOrDefault(tc => tc.TeacherId == teacherId);
        if (teacherClass == null)
            return ServiceResult.Fail("Bạn không phụ trách lớp học này.");

        classEntity.TeacherClasses.Remove(teacherClass);
        await _classRepository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DissolveClassAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult.Fail("Không tìm thấy lớp học.");

        if (classEntity.CreatedBy != teacherId)
            return ServiceResult.Fail("Bạn chỉ có thể giải tán lớp do chính mình tạo.");

        _classRepository.Remove(classEntity);
        await _classRepository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<ImportStudentsResult>> ImportStudentsFromExcelAsync(int classId, Stream excelStream)
    {
        var classEntity = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (classEntity == null)
            return ServiceResult<ImportStudentsResult>.Fail("Không tìm thấy lớp học.");

        var result = new ImportStudentsResult();

        // Đọc các hàng từ Excel
        List<(string FullName, string Email, string? Phone)> rows;
        try
        {
            using var wb = new XLWorkbook(excelStream);
            var ws = wb.Worksheet(1);
            rows = ws.RowsUsed()
                .Skip(1) // bỏ header
                .Select(row => (
                    FullName: row.Cell(1).GetString().Trim(),
                    Email: row.Cell(2).GetString().Trim(),
                    Phone: row.Cell(3).GetString().Trim() is { Length: > 0 } p ? p : (string?)null
                ))
                .Where(r => !string.IsNullOrWhiteSpace(r.Email))
                .DistinctBy(r => r.Email.ToLowerInvariant())
                .ToList();
        }
        catch
        {
            return ServiceResult<ImportStudentsResult>.Fail("File Excel không hợp lệ hoặc bị lỗi. Cần đủ 3 cột: Họ và tên, Email, Số điện thoại.");
        }

        foreach (var (fullName, email, phone) in rows)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                result.Errors.Add($"Hàng email '{email}': thiếu họ và tên.");
                result.InvalidRows++;
                continue;
            }

            // Tìm hoặc tạo user
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null)
            {
                // Tạo mới student với password mặc định là email
                user = new User
                {
                    FullName = fullName,
                    Email = email,
                    PhoneNumber = phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(email),
                    Role = "Student",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                await _userRepository.AddAsync(user);
                await _userRepository.SaveChangesAsync();
                result.NewUsersCreated++;
            }
            else if (user.Role != "Student")
            {
                result.Errors.Add($"{email}: tài khoản này không phải Student (role: {user.Role}).");
                result.InvalidRows++;
                continue;
            }
            else if (!user.IsActive)
            {
                result.Errors.Add($"{email}: tài khoản đang bị khóa.");
                result.InvalidRows++;
                continue;
            }

            // Kiểm tra đã trong lớp chưa
            if (await _classRepository.IsStudentInClassAsync(user.Id, classId))
            {
                result.AlreadyInClass++;
                continue;
            }

            classEntity.StudentClasses.Add(new StudentClass
            {
                StudentId = user.Id,
                ClassId = classId,
                JoinedAt = DateTime.Now,
                Status = "Active"
            });
            result.Added++;
        }

        if (result.Added > 0)
            await _classRepository.SaveChangesAsync();

        return ServiceResult<ImportStudentsResult>.Ok(result);
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
        CreatedBy = classEntity.CreatedBy,
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
            }).ToList() ?? new(),
        Teacher = classEntity.TeacherClasses?
            .Where(tc => tc.Teacher != null)
            .Select(tc => new TeacherInClassResponse
            {
                TeacherId = tc.TeacherId,
                FullName = tc.Teacher!.FullName,
                Email = tc.Teacher.Email,
                PhoneNumber = tc.Teacher.PhoneNumber,
                AssignedAt = tc.AssignedAt
            }).FirstOrDefault()
    };
}
