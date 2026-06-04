using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Subjects;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class SubjectService : ISubjectService
{
    private static readonly string[] ValidStatuses = { "Active", "Inactive" };
    private readonly ISubjectRepository _subjectRepository;

    public SubjectService(ISubjectRepository subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public IQueryable<SubjectODataResponse> GetODataQueryable()
    {
        return _subjectRepository.GetQueryable().Select(subject => new SubjectODataResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Status = subject.Status,
            QuestionCount = subject.Questions.Count(q => !q.IsDeleted),
            CreatedAt = subject.CreatedAt
        });
    }

    public async Task<SubjectResponse?> GetByIdAsync(int id)
    {
        var subject = await _subjectRepository.GetActiveByIdAsync(id);
        return subject == null ? null : MapToResponse(subject);
    }

    public async Task<ServiceResult<SubjectResponse>> CreateAsync(CreateSubjectRequest request)
    {
        if (!IsValidStatus(request.Status))
        {
            return ServiceResult<SubjectResponse>.Fail("Trạng thái môn học không hợp lệ.");
        }

        if (await _subjectRepository.CodeExistsAsync(request.Code))
        {
            return ServiceResult<SubjectResponse>.Fail("Mã môn học đã tồn tại.");
        }

        var subject = new Subject
        {
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            CreatedAt = DateTime.Now
        };

        await _subjectRepository.AddAsync(subject);
        await _subjectRepository.SaveChangesAsync();

        return ServiceResult<SubjectResponse>.Ok(MapToResponse(subject));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateSubjectRequest request)
    {
        if (!IsValidStatus(request.Status))
        {
            return ServiceResult.Fail("Trạng thái môn học không hợp lệ.");
        }

        var subject = await _subjectRepository.GetActiveByIdAsync(id);

        if (subject == null)
        {
            return ServiceResult.Fail("Không tìm thấy môn học.");
        }

        subject.Name = request.Name;
        subject.Description = request.Description;
        subject.Status = request.Status;
        subject.UpdatedAt = DateTime.Now;

        _subjectRepository.Update(subject);
        await _subjectRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var subject = await _subjectRepository.GetActiveByIdAsync(id);

        if (subject == null)
        {
            return ServiceResult.Fail("Không tìm thấy môn học.");
        }

        if (await _subjectRepository.HasActiveQuestionsAsync(id))
        {
            return ServiceResult.Fail("Không thể xóa môn học đang có câu hỏi active.");
        }

        subject.IsDeleted = true;
        subject.UpdatedAt = DateTime.Now;

        _subjectRepository.Update(subject);
        await _subjectRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private static bool IsValidStatus(string status)
    {
        return ValidStatuses.Contains(status);
    }

    private static SubjectResponse MapToResponse(Subject subject)
    {
        return new SubjectResponse
        {
            Id = subject.Id,
            Code = subject.Code,
            Name = subject.Name,
            Description = subject.Description,
            Status = subject.Status,
            QuestionCount = subject.Questions.Count(q => !q.IsDeleted),
            CreatedAt = subject.CreatedAt,
            UpdatedAt = subject.UpdatedAt
        };
    }
}
