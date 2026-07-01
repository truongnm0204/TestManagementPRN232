using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.ExamAttempts;
using TestManagement.BAL.Hubs;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services;

public class ExamAttemptService : IExamAttemptService
{
    private readonly IExamAttemptRepository _attemptRepository;
    private readonly IExamRepository _examRepository;
    private readonly IExamAssignmentRepository _assignmentRepository;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<ExamMonitorHub> _monitorHub;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ExamAttemptService(
        IExamAttemptRepository attemptRepository,
        IExamRepository examRepository,
        IExamAssignmentRepository assignmentRepository,
        INotificationService notificationService,
        IHubContext<ExamMonitorHub> monitorHub)
    {
        _attemptRepository = attemptRepository;
        _examRepository = examRepository;
        _assignmentRepository = assignmentRepository;
        _notificationService = notificationService;
        _monitorHub = monitorHub;
    }

    // ─── GetMyExams ───────────────────────────────────────────────────────────
    public async Task<ServiceResult<List<MyExamResponse>>> GetMyExamsAsync(int studentId)
    {
        // Lấy tất cả assignment có class chứa student này
        var assignments = await _assignmentRepository.GetByStudentAsync(studentId);
        if (!assignments.Any())
            return ServiceResult<List<MyExamResponse>>.Ok(new List<MyExamResponse>());

        var examIds = assignments.Select(a => a.ExamId).Distinct().ToList();
        var attempts = await _attemptRepository.GetByStudentAsync(studentId);
        var attemptsByExam = attempts.GroupBy(a => a.ExamId).ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<MyExamResponse>();
        var now = DateTime.Now;

        foreach (var assignment in assignments)
        {
            var exam = assignment.Exam;
            if (exam == null || !exam.IsPublished) continue;

            var snapshot = DeserializeSnapshot(exam.PublishedSnapshotJson);
            var examAttempts = attemptsByExam.GetValueOrDefault(exam.Id, new List<ExamAttempt>());
            var done = examAttempts.Count(a => a.Status != "Abandoned");
            var latest = examAttempts.OrderByDescending(a => a.StartedAt).FirstOrDefault();

            bool withinWindow = (!exam.AvailableFrom.HasValue || now >= exam.AvailableFrom.Value)
                             && (!exam.AvailableTo.HasValue || now <= exam.AvailableTo.Value);

            bool canStart = withinWindow
                         && done < exam.AttemptLimit
                         && (latest == null || latest.Status != "InProgress");

            // Nếu đang có InProgress thì cho resume (canStart = true)
            if (latest?.Status == "InProgress") canStart = true;

            result.Add(new MyExamResponse
            {
                ExamId = exam.Id,
                Title = exam.Title,
                SubjectCode = exam.Subject?.Code ?? string.Empty,
                SubjectName = exam.Subject?.Name ?? string.Empty,
                ClassName = assignment.Class?.ClassName ?? string.Empty,
                DurationMinutes = exam.DurationMinutes,
                QuestionCount = snapshot?.QuestionCount ?? 0,
                TotalPoints = snapshot?.TotalPoints ?? 0,
                AttemptLimit = exam.AttemptLimit,
                AttemptsDone = done,
                AvailableFrom = exam.AvailableFrom,
                AvailableTo = exam.AvailableTo,
                LatestAttemptStatus = latest?.Status,
                LatestAttemptId = latest?.Id,
                CanStart = canStart
            });
        }

        return ServiceResult<List<MyExamResponse>>.Ok(result.DistinctBy(x => x.ExamId).ToList());
    }

    // ─── Start / Resume ───────────────────────────────────────────────────────
    public async Task<ServiceResult<StartAttemptResponse>> StartAsync(int examId, int studentId)
    {
        var exam = await _examRepository.GetDetailAsync(examId);
        if (exam == null || !exam.IsPublished)
            return ServiceResult<StartAttemptResponse>.Fail("Không tìm thấy đề thi hoặc đề chưa được publish.");

        // Kiểm tra cửa sổ thời gian
        var now = DateTime.Now;
        if (exam.AvailableFrom.HasValue && now < exam.AvailableFrom.Value)
            return ServiceResult<StartAttemptResponse>.Fail("Đề thi chưa mở.");
        if (exam.AvailableTo.HasValue && now > exam.AvailableTo.Value)
            return ServiceResult<StartAttemptResponse>.Fail("Đề thi đã hết thời hạn.");

        // Kiểm tra student thuộc lớp được giao
        var assignments = await _assignmentRepository.GetByStudentAsync(studentId);
        if (!assignments.Any(a => a.ExamId == examId))
            return ServiceResult<StartAttemptResponse>.Fail("Bạn không có quyền làm đề thi này.");

        // Resume nếu đang có InProgress
        var active = await _attemptRepository.GetActiveAttemptAsync(examId, studentId);
        if (active != null)
            return ServiceResult<StartAttemptResponse>.Ok(BuildStartResponse(exam, active));

        // Kiểm tra số lần làm
        var done = await _attemptRepository.CountAttemptsAsync(examId, studentId);
        if (done >= exam.AttemptLimit)
            return ServiceResult<StartAttemptResponse>.Fail($"Bạn đã dùng hết {exam.AttemptLimit} lượt làm bài.");

        // Tạo attempt mới
        var attempt = new ExamAttempt
        {
            ExamId = examId,
            StudentId = studentId,
            StartedAt = DateTime.Now,
            Status = "InProgress",
            QuestionSnapshotJson = BuildAttemptSnapshot(exam),
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        await _attemptRepository.AddAsync(attempt);
        await _attemptRepository.SaveChangesAsync();

        // Reload để có StudentAnswers collection
        var created = await _attemptRepository.GetDetailAsync(attempt.Id);

        // Push realtime: student bắt đầu làm → cập nhật bảng monitor của teacher
        await PushMonitorStatusAsync(created!, "InProgress", null);

        return ServiceResult<StartAttemptResponse>.Ok(BuildStartResponse(exam, created!));
    }

    // ─── GetAttempt (resume theo attemptId, dùng khi refresh trang làm bài) ─────
    public async Task<ServiceResult<StartAttemptResponse>> GetAttemptAsync(int attemptId, int studentId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId);
        if (attempt == null || attempt.StudentId != studentId)
            return ServiceResult<StartAttemptResponse>.Fail("Không tìm thấy lần làm bài.");
        if (attempt.Status != "InProgress")
            return ServiceResult<StartAttemptResponse>.Fail("Lần làm bài này đã kết thúc.");

        var exam = attempt.Exam;
        if (exam == null)
            return ServiceResult<StartAttemptResponse>.Fail("Không tìm thấy đề thi.");

        return ServiceResult<StartAttemptResponse>.Ok(BuildStartResponse(exam, attempt));
    }

    // ─── SaveAnswer ───────────────────────────────────────────────────────────
    public async Task<ServiceResult> SaveAnswerAsync(int attemptId, SaveAnswerRequest request, int studentId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId);
        if (attempt == null || attempt.StudentId != studentId)
            return ServiceResult.Fail("Không tìm thấy lần làm bài.");
        if (attempt.Status != "InProgress")
            return ServiceResult.Fail("Lần làm bài này đã kết thúc.");

        var existing = attempt.StudentAnswers.FirstOrDefault(a => a.QuestionId == request.QuestionId);
        if (existing != null)
        {
            // Chỉ ghi lịch sử khi đáp án thực sự đổi
            if (existing.SelectedOptionId != request.SelectedOptionId)
            {
                existing.AnswerHistoryJson = AppendHistory(existing.AnswerHistoryJson, existing.SelectedOptionId, request.SelectedOptionId);
                existing.SelectedOptionId = request.SelectedOptionId;
                existing.AnsweredAt = DateTime.Now;
                existing.UpdatedAt = DateTime.Now;
            }
        }
        else
        {
            attempt.StudentAnswers.Add(new StudentAnswer
            {
                ExamAttemptId = attemptId,
                QuestionId = request.QuestionId,
                SelectedOptionId = request.SelectedOptionId,
                AnswerHistoryJson = AppendHistory(null, null, request.SelectedOptionId),
                AnsweredAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            });
        }

        attempt.UpdatedAt = DateTime.Now;
        _attemptRepository.Update(attempt);
        await _attemptRepository.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ─── Submit ───────────────────────────────────────────────────────────────
    public async Task<ServiceResult<SubmitAttemptResponse>> SubmitAsync(int attemptId, int studentId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId);
        if (attempt == null || attempt.StudentId != studentId)
            return ServiceResult<SubmitAttemptResponse>.Fail("Không tìm thấy lần làm bài.");
        if (attempt.Status != "InProgress")
            return ServiceResult<SubmitAttemptResponse>.Fail("Lần làm bài này đã kết thúc.");

        var exam = attempt.Exam!;
        var answerKey = DeserializeAnswerKey(exam.AnswerKeyJson);
        var snapshot = DeserializeSnapshot(exam.PublishedSnapshotJson)!;
        var answerMap = attempt.StudentAnswers.ToDictionary(a => a.QuestionId);
        var keyMap = answerKey.ToDictionary(k => k.QuestionId);

        int correct = 0, wrong = 0, skipped = 0;
        decimal totalScore = 0;
        var details = new List<AnswerDetailItem>();

        foreach (var q in snapshot.Questions.OrderBy(q => q.SortOrder))
        {
            answerMap.TryGetValue(q.QuestionId, out var answer);
            keyMap.TryGetValue(q.QuestionId, out var key);

            bool? isCorrect = null;
            decimal score = 0;

            if (answer?.SelectedOptionId != null && key != null)
            {
                isCorrect = key.CorrectOptionIds.Contains(answer.SelectedOptionId.Value);
                score = isCorrect == true ? key.Points : 0;
            }

            if (answer == null || answer.SelectedOptionId == null) skipped++;
            else if (isCorrect == true) correct++;
            else wrong++;

            totalScore += score;

            // Update StudentAnswer với kết quả
            if (answer != null)
            {
                answer.IsCorrect = isCorrect;
                answer.Score = score;
            }

            details.Add(new AnswerDetailItem
            {
                QuestionId = q.QuestionId,
                SortOrder = q.SortOrder,
                Content = q.Content,
                QuestionType = q.QuestionType,
                Points = q.Points,
                SelectedOptionId = answer?.SelectedOptionId,
                IsCorrect = isCorrect,
                Score = score,
                CorrectOptionIds = exam.ShowCorrectAnswers ? (key?.CorrectOptionIds ?? new List<int>()) : new List<int>(),
                Options = q.Options
            });
        }

        attempt.TotalScore = totalScore;
        attempt.CorrectCount = correct;
        attempt.WrongCount = wrong;
        attempt.SubmittedAt = DateTime.Now;
        attempt.Status = "Submitted";
        attempt.UpdatedAt = DateTime.Now;

        _attemptRepository.Update(attempt);
        await _attemptRepository.SaveChangesAsync();

        // Push realtime: cập nhật monitor + thông báo cho teacher chấm
        await PushMonitorStatusAsync(attempt, "Submitted", totalScore);
        await NotifyTeachersOnSubmitAsync(exam, attempt);

        return ServiceResult<SubmitAttemptResponse>.Ok(new SubmitAttemptResponse
        {
            AttemptId = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            TotalScore = totalScore,
            MaxScore = snapshot.TotalPoints,
            CorrectCount = correct,
            WrongCount = wrong,
            SkippedCount = skipped,
            QuestionCount = snapshot.QuestionCount,
            SubmittedAt = attempt.SubmittedAt!.Value,
            ShowResultsImmediately = exam.ShowResultsImmediately,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            Details = exam.ShowResultsImmediately ? details : new List<AnswerDetailItem>()
        });
    }

    // ─── GetResult ────────────────────────────────────────────────────────────
    public async Task<ServiceResult<SubmitAttemptResponse>> GetResultAsync(int attemptId, int studentId)
    {
        var attempt = await _attemptRepository.GetDetailAsync(attemptId);
        if (attempt == null || attempt.StudentId != studentId)
            return ServiceResult<SubmitAttemptResponse>.Fail("Không tìm thấy kết quả.");
        if (attempt.Status != "Submitted")
            return ServiceResult<SubmitAttemptResponse>.Fail("Bài thi chưa được nộp.");

        var exam = attempt.Exam!;
        if (!exam.ShowResultsImmediately)
            return ServiceResult<SubmitAttemptResponse>.Fail("Kết quả chưa được công bố.");

        var snapshot = DeserializeSnapshot(exam.PublishedSnapshotJson)!;
        var answerMap = attempt.StudentAnswers.ToDictionary(a => a.QuestionId);
        var keyMap = DeserializeAnswerKey(exam.AnswerKeyJson).ToDictionary(k => k.QuestionId);

        var details = snapshot.Questions.OrderBy(q => q.SortOrder).Select(q =>
        {
            answerMap.TryGetValue(q.QuestionId, out var answer);
            keyMap.TryGetValue(q.QuestionId, out var key);
            return new AnswerDetailItem
            {
                QuestionId = q.QuestionId,
                SortOrder = q.SortOrder,
                Content = q.Content,
                QuestionType = q.QuestionType,
                Points = q.Points,
                SelectedOptionId = answer?.SelectedOptionId,
                IsCorrect = answer?.IsCorrect,
                Score = answer?.Score,
                CorrectOptionIds = exam.ShowCorrectAnswers ? (key?.CorrectOptionIds ?? new List<int>()) : new List<int>(),
                Options = q.Options
            };
        }).ToList();

        return ServiceResult<SubmitAttemptResponse>.Ok(new SubmitAttemptResponse
        {
            AttemptId = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            TotalScore = attempt.TotalScore ?? 0,
            MaxScore = snapshot.TotalPoints,
            CorrectCount = attempt.CorrectCount,
            WrongCount = attempt.WrongCount,
            SkippedCount = snapshot.QuestionCount - attempt.CorrectCount - attempt.WrongCount,
            QuestionCount = snapshot.QuestionCount,
            SubmittedAt = attempt.SubmittedAt!.Value,
            ShowResultsImmediately = exam.ShowResultsImmediately,
            ShowCorrectAnswers = exam.ShowCorrectAnswers,
            Details = details
        });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private static StartAttemptResponse BuildStartResponse(Exam exam, ExamAttempt attempt)
    {
        // Đọc snapshot CỦA ATTEMPT (đã shuffle khi start) — không phải snapshot gốc của exam
        var snapshot = DeserializeSnapshot(attempt.QuestionSnapshotJson)
                       ?? DeserializeSnapshot(exam.PublishedSnapshotJson)
                       ?? new SnapshotDto();
        var savedAnswers = attempt.StudentAnswers
            .Where(a => a.SelectedOptionId != null)
            .ToDictionary(a => a.QuestionId, a => a.SelectedOptionId!.Value);

        return new StartAttemptResponse
        {
            AttemptId = attempt.Id,
            ExamId = exam.Id,
            ExamTitle = exam.Title,
            DurationMinutes = exam.DurationMinutes,
            StartedAt = attempt.StartedAt,
            DeadlineAt = attempt.StartedAt.AddMinutes(exam.DurationMinutes),
            ShuffleOptions = exam.ShuffleOptions,
            QuestionCount = snapshot.QuestionCount,
            TotalPoints = snapshot.TotalPoints,
            Questions = snapshot.Questions.OrderBy(q => q.SortOrder).ToList(),
            SavedAnswers = savedAnswers
        };
    }

    // Tạo snapshot riêng cho attempt: xáo câu hỏi/đáp án nếu exam bật cờ tương ứng
    private static string? BuildAttemptSnapshot(Exam exam)
    {
        var snap = DeserializeSnapshot(exam.PublishedSnapshotJson);
        if (snap == null) return exam.PublishedSnapshotJson;

        if (exam.ShuffleQuestions)
        {
            snap.Questions = snap.Questions.OrderBy(_ => Guid.NewGuid()).ToList();
        }

        // Gán lại SortOrder câu hỏi theo thứ tự hiện tại để client render ổn định
        for (int i = 0; i < snap.Questions.Count; i++)
        {
            var q = snap.Questions[i];
            q.SortOrder = i + 1;

            if (exam.ShuffleOptions)
            {
                q.Options = q.Options.OrderBy(_ => Guid.NewGuid()).ToList();
            }
            for (int j = 0; j < q.Options.Count; j++)
            {
                q.Options[j].SortOrder = j + 1;
            }
        }

        return JsonSerializer.Serialize(snap, JsonOptions);
    }

    // Attempt đã hết thời gian làm bài chưa (deadline = StartedAt + DurationMinutes)
    private static bool IsExpired(ExamAttempt attempt, Exam exam)
    {
        return DateTime.Now > attempt.StartedAt.AddMinutes(exam.DurationMinutes);
    }

    private static SnapshotDto? DeserializeSnapshot(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<SnapshotDto>(json, JsonOptions); }
        catch { return null; }
    }

    // Push trạng thái student tới các phòng giám sát (mọi lớp được giao đề mà student thuộc)
    private async Task PushMonitorStatusAsync(ExamAttempt attempt, string status, decimal? score)
    {
        var assignments = await _assignmentRepository.GetByStudentAsync(attempt.StudentId);
        var classIds = assignments.Where(a => a.ExamId == attempt.ExamId).Select(a => a.ClassId).Distinct();

        var payload = new
        {
            studentId = attempt.StudentId,
            studentName = attempt.Student?.FullName ?? string.Empty,
            attemptId = attempt.Id,
            status,
            score,
            at = DateTime.Now
        };

        foreach (var classId in classIds)
        {
            await _monitorHub.Clients.Group($"monitor-{attempt.ExamId}-{classId}")
                .SendAsync("StudentStatusChanged", payload);
        }
    }

    // Thông báo cho (các) teacher đã giao đề khi có student nộp bài
    private async Task NotifyTeachersOnSubmitAsync(Exam exam, ExamAttempt attempt)
    {
        var assignments = await _assignmentRepository.GetByExamIdAsync(exam.Id);
        var teacherIds = assignments.Select(a => a.AssignedBy).Distinct();
        var studentName = attempt.Student?.FullName ?? "Học viên";

        foreach (var teacherId in teacherIds)
        {
            await _notificationService.CreateAndPushAsync(
                teacherId,
                "Có bài thi vừa nộp",
                $"{studentName} đã nộp bài: {exam.Title}",
                "exam_submitted");
        }
    }

    private static List<AnswerKeyItemDto> DeserializeAnswerKey(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new List<AnswerKeyItemDto>();
        try { return JsonSerializer.Deserialize<List<AnswerKeyItemDto>>(json, JsonOptions) ?? new(); }
        catch { return new List<AnswerKeyItemDto>(); }
    }

    // Append 1 entry vào lịch sử đổi đáp án, trả về JSON mới
    private static string AppendHistory(string? currentJson, int? from, int? to)
    {
        List<AnswerHistoryEntry> history;
        if (string.IsNullOrWhiteSpace(currentJson))
        {
            history = new List<AnswerHistoryEntry>();
        }
        else
        {
            try { history = JsonSerializer.Deserialize<List<AnswerHistoryEntry>>(currentJson, JsonOptions) ?? new(); }
            catch { history = new List<AnswerHistoryEntry>(); }
        }

        history.Add(new AnswerHistoryEntry { At = DateTime.Now, From = from, To = to });
        return JsonSerializer.Serialize(history, JsonOptions);
    }
}
