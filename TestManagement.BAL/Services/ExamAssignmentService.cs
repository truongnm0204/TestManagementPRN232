using Microsoft.EntityFrameworkCore;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAssignments;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class ExamAssignmentService : IExamAssignmentService
{
    private readonly IExamAssignmentRepository _assignmentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IClassRepository _classRepository;

    public ExamAssignmentService(
        IExamAssignmentRepository assignmentRepository,
        IExamRepository examRepository,
        IClassRepository classRepository)
    {
        _assignmentRepository = assignmentRepository;
        _examRepository = examRepository;
        _classRepository = classRepository;
    }

    public async Task<ServiceResult<List<ExamAssignmentResponse>>> GetByExamIdAsync(int examId)
    {
        var exam = await _examRepository.GetDetailAsync(examId);
        if (exam == null)
        {
            return ServiceResult<List<ExamAssignmentResponse>>.Fail("Không tìm thấy đề thi.");
        }

        var assignments = await _assignmentRepository.GetByExamIdAsync(examId);
        return ServiceResult<List<ExamAssignmentResponse>>.Ok(assignments.Select(MapToResponse).ToList());
    }

    public async Task<ServiceResult<ExamAssignmentResponse>> AssignAsync(int examId, AssignExamRequest request, int? currentUserId)
    {
        if (currentUserId == null)
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Không xác định được người giao đề thi.");
        }

        var exam = await _examRepository.GetDetailAsync(examId);
        if (exam == null)
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Không tìm thấy đề thi.");
        }

        if (!exam.IsPublished || exam.Status != "Published")
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Chỉ được giao đề thi đã publish.");
        }

        var classEntity = await _classRepository.GetByIdAsync(request.ClassId);
        if (classEntity == null)
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Không tìm thấy lớp học.");
        }

        if (classEntity.Status != "Active")
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Chỉ được giao đề cho lớp đang Active.");
        }

        if (await _assignmentRepository.ExistsAsync(examId, request.ClassId))
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Đề thi đã được giao cho lớp này.");
        }

        var assignment = new ExamAssignment
        {
            ExamId = examId,
            ClassId = request.ClassId,
            AssignedBy = currentUserId.Value,
            AssignedAt = DateTime.Now
        };

        try
        {
            await _assignmentRepository.AddAsync(assignment);
            await _assignmentRepository.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Đề thi đã được giao cho lớp này.");
        }

        var created = await _assignmentRepository.GetDetailAsync(assignment.Id);
        if (created == null)
        {
            return ServiceResult<ExamAssignmentResponse>.Fail("Giao đề thành công nhưng không tải được dữ liệu chi tiết.");
        }

        return ServiceResult<ExamAssignmentResponse>.Ok(MapToResponse(created));
    }

    public async Task<ServiceResult> RemoveAsync(int examId, int assignmentId)
    {
        var assignment = await _assignmentRepository.GetDetailAsync(assignmentId);
        if (assignment == null || assignment.ExamId != examId)
        {
            return ServiceResult.Fail("Không tìm thấy phân công đề thi.");
        }

        _assignmentRepository.Remove(assignment);
        await _assignmentRepository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    private static ExamAssignmentResponse MapToResponse(ExamAssignment assignment)
    {
        return new ExamAssignmentResponse
        {
            Id = assignment.Id,
            ExamId = assignment.ExamId,
            ExamTitle = assignment.Exam?.Title ?? string.Empty,
            ClassId = assignment.ClassId,
            ClassCode = assignment.Class?.ClassCode ?? string.Empty,
            ClassName = assignment.Class?.ClassName ?? string.Empty,
            AssignedBy = assignment.AssignedBy,
            AssignerName = assignment.Assigner?.FullName ?? string.Empty,
            AssignedAt = assignment.AssignedAt
        };
    }
}
