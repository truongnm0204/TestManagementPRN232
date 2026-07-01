using System.Text.Json;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAttempts;
using TestManagement.BAL.DTOs.ExamResults;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class ExamResultService : IExamResultService
{
    private readonly IExamAttemptRepository _attemptRepository;
    private readonly IExamAssignmentRepository _assignmentRepository;
    private readonly IExamRepository _examRepository;
    private readonly IClassRepository _classRepository;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExamResultService(
        IExamAttemptRepository attemptRepository,
        IExamAssignmentRepository assignmentRepository,
        IExamRepository examRepository,
        IClassRepository classRepository)
    {
        _attemptRepository = attemptRepository;
        _assignmentRepository = assignmentRepository;
        _examRepository = examRepository;
        _classRepository = classRepository;
    }

    // ─── Danh sách cặp (đề × lớp) đã giao ──────────────────────────────────────
    public async Task<ServiceResult<List<ExamAssignmentSummaryDto>>> GetTeacherAssignmentsAsync()
    {
        var assignments = await _assignmentRepository.GetAllWithDetailsAsync();
        var result = assignments.Select(a => new ExamAssignmentSummaryDto
        {
            ExamId = a.ExamId,
            ExamTitle = a.Exam?.Title ?? string.Empty,
            SubjectCode = a.Exam?.Subject?.Code ?? string.Empty,
            ClassId = a.ClassId,
            ClassName = a.Class?.ClassName ?? string.Empty,
            AssignedAt = a.AssignedAt
        }).ToList();

        return ServiceResult<List<ExamAssignmentSummaryDto>>.Ok(result);
    }

    // ─── Kết quả của cả lớp cho một đề ─────────────────────────────────────────
    public async Task<ServiceResult<ClassResultDto>> GetClassResultAsync(int examId, int classId)
    {
        var exam = await _examRepository.GetDetailAsync(examId);
        if (exam == null)
            return ServiceResult<ClassResultDto>.Fail("Không tìm thấy đề thi.");

        var assigned = await _assignmentRepository.ExistsAsync(examId, classId);
        if (!assigned)
            return ServiceResult<ClassResultDto>.Fail("Đề thi chưa được giao cho lớp này.");

        var cls = await _classRepository.GetByIdWithStudentsAsync(classId);
        if (cls == null)
            return ServiceResult<ClassResultDto>.Fail("Không tìm thấy lớp học.");

        var snapshot = DeserializeSnapshot(exam.PublishedSnapshotJson);

        // Map attempt mới nhất theo student (ưu tiên attempt đã Submitted)
        var attempts = await _attemptRepository.GetByExamIdAsync(examId);
        var attemptByStudent = attempts
            .GroupBy(a => a.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderByDescending(a => a.Status == "Submitted")
                      .ThenByDescending(a => a.StartedAt)
                      .First());

        var rows = new List<ClassResultRowDto>();
        foreach (var sc in cls.StudentClasses.Where(x => x.Status == "Active"))
        {
            var student = sc.Student;
            if (student == null) continue;

            attemptByStudent.TryGetValue(student.Id, out var attempt);
            rows.Add(new ClassResultRowDto
            {
                StudentId = student.Id,
                StudentName = student.FullName,
                AttemptId = attempt?.Id,
                Status = attempt?.Status ?? "NotStarted",
                TotalScore = attempt?.TotalScore,
                CorrectCount = attempt?.CorrectCount ?? 0,
                WrongCount = attempt?.WrongCount ?? 0,
                SubmittedAt = attempt?.SubmittedAt
            });
        }

        return ServiceResult<ClassResultDto>.Ok(new ClassResultDto
        {
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            ClassId = cls.Id,
            ClassName = cls.ClassName,
            MaxScore = snapshot?.TotalPoints ?? 0,
            QuestionCount = snapshot?.QuestionCount ?? 0,
            Rows = rows.OrderBy(r => r.StudentName).ToList()
        });
    }

    // ─── Lịch sử đổi đáp án của một attempt ────────────────────────────────────
    public async Task<ServiceResult<List<AnswerHistoryViewDto>>> GetAttemptHistoryAsync(int attemptId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId);
        if (attempt == null)
            return ServiceResult<List<AnswerHistoryViewDto>>.Fail("Không tìm thấy lần làm bài.");

        var snapshot = DeserializeSnapshot(attempt.QuestionSnapshotJson);
        if (snapshot == null)
            return ServiceResult<List<AnswerHistoryViewDto>>.Ok(new List<AnswerHistoryViewDto>());

        // questionId -> (sortOrder, content, optionId->label)
        var qMap = snapshot.Questions.ToDictionary(q => q.QuestionId);
        var answerMap = attempt.StudentAnswers.ToDictionary(a => a.QuestionId);

        var result = new List<AnswerHistoryViewDto>();
        foreach (var q in snapshot.Questions.OrderBy(q => q.SortOrder))
        {
            var changes = new List<AnswerHistoryChangeDto>();
            if (answerMap.TryGetValue(q.QuestionId, out var ans) && !string.IsNullOrWhiteSpace(ans.AnswerHistoryJson))
            {
                var history = DeserializeHistory(ans.AnswerHistoryJson);
                var optLabels = q.Options.ToDictionary(o => o.OptionId, o => o.Label);
                changes = history.Select(h => new AnswerHistoryChangeDto
                {
                    At = h.At,
                    FromLabel = h.From.HasValue && optLabels.TryGetValue(h.From.Value, out var fl) ? fl : null,
                    ToLabel = h.To.HasValue && optLabels.TryGetValue(h.To.Value, out var tl) ? tl : null
                }).ToList();
            }

            result.Add(new AnswerHistoryViewDto
            {
                QuestionId = q.QuestionId,
                SortOrder = q.SortOrder,
                Content = q.Content,
                Changes = changes
            });
        }

        return ServiceResult<List<AnswerHistoryViewDto>>.Ok(result);
    }

    // ─── Helpers ───────────────────────────────────────────────────────────────
    private static SnapshotDto? DeserializeSnapshot(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<SnapshotDto>(json, JsonOptions); }
        catch { return null; }
    }

    private static List<AnswerHistoryEntry> DeserializeHistory(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<AnswerHistoryEntry>();
        try { return JsonSerializer.Deserialize<List<AnswerHistoryEntry>>(json, JsonOptions) ?? new(); }
        catch { return new List<AnswerHistoryEntry>(); }
    }
}
