using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Exams;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;

namespace TestManagement.BAL.Services
{
    public class ExamService : IExamService
    {
        private readonly IExamRepository _examRepository;
        private readonly ISubjectRepository _subjectRepository;

        private static readonly string[] EditableStatuses = { "Draft" };

        public ExamService(IExamRepository repository, ISubjectRepository subjectRepository)
        {
            _examRepository = repository;
            _subjectRepository = subjectRepository;
        }
        public async Task<ServiceResult<ExamResponse>> CreateAsync(CreateExamRequest request, int? currentUserId)
        {
            // check currentUserId is null
            if(currentUserId == null)
            {
                return ServiceResult<ExamResponse>.Fail("Không xác định được người tạo đề thi.");
            }
            //validate ExamRequest return error message if invalid, otherwise return null
            var validationError = await ValidateExamRequestAsync(request.SubjectId, request.DurationMinutes, request.AttemptLimit, request.AvailableFrom, request.AvailableTo);
            // create new exam and save
            if(validationError != null)
            {
                return ServiceResult<ExamResponse>.Fail(validationError);
            }
            var exam = new Exam
            {
                SubjectId = request.SubjectId,
                CreatedBy = currentUserId.Value,
                Title = request.Title.Trim(),
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                Status = "Draft",
                AvailableFrom = request.AvailableFrom,
                AvailableTo = request.AvailableTo,
                AttemptLimit = request.AttemptLimit,
                ShuffleQuestions = request.ShuffleQuestions,
                ShuffleOptions = request.ShuffleOptions,
                ShowResultsImmediately = request.ShowResultsImmediately,
                ShowCorrectAnswers = request.ShowCorrectAnswers,
                QuestionItemsJson = "[]",
                IsPublished = false,
                IsDeleted = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            await _examRepository.AddAsync(exam);
            await _examRepository.SaveChangesAsync();
            // check doan nay

            var createdExam = await _examRepository.GetDetailAsync(exam.Id);
            if (createdExam == null)
            {
                return ServiceResult<ExamResponse>.Fail("Tạo đề thi thành công nhưng không tải được dữ liệu chi tiết.");
            }
            return ServiceResult<ExamResponse>.Ok(MaptoResponse(createdExam ?? exam));
        }

        private async Task<string?> ValidateExamRequestAsync(int subjectId, 
            int durationMinutes, int attemptLimit,
            DateTime? availableFrom, DateTime? availableTo)
        {
            // check valid subjectID
            var subject = await _subjectRepository.GetByIdAsync(subjectId);
            if(subject == null || subject.Status != "Active") 
            {
                return "Môn học không tìm thấy hoặc không tồn tại";
            }
            //Check duration > 0
            if(durationMinutes <= 0)
            {
                return "Thời gian làm bài phải lớn hơn 0";
            }

            // check attemptLimit >0
            if (attemptLimit <= 0)
            {
                return "Số lần làm bài phải lớn hơn 0";
            }
            // check availableFrom > availableTo
            if(availableFrom.HasValue && availableTo.HasValue && availableFrom >= availableTo) {
                return "Thowif gian bắt đầu phải nhỏ hơn thời gian kết thúc";
            }

            return null;
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var exam = await _examRepository.GetDetailAsync(id);

            if (exam == null)
            {
                return ServiceResult.Fail("Không tìm thấy đề thi.");
            }

            if (!EditableStatuses.Contains(exam.Status))
            {
                return ServiceResult.Fail("Chỉ được xóa đề thi ở trạng thái Draft.");
            }

            exam.IsDeleted = true;
            exam.UpdatedAt = DateTime.Now;

            _examRepository.Update(exam);
            await _examRepository.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<ExamResponse?> GetByIdAsync(int id)
        {
            var exam = await _examRepository.GetDetailAsync(id);
            return exam == null ? null : MaptoResponse(exam);
        }

        private static ExamResponse MaptoResponse(Exam exam)
        {
            return new ExamResponse
            {
                Id = exam.Id,
                SubjectId = exam.SubjectId,
                SubjectCode = exam.Subject?.Code ?? string.Empty,
                SubjectName = exam.Subject?.Name ?? string.Empty,
                CreatedBy = exam.CreatedBy,
                CreatorName = exam.Creator?.FullName ?? string.Empty,
                Title = exam.Title,
                Description = exam.Description,
                DurationMinutes = exam.DurationMinutes,
                Status = exam.Status,
                AvailableFrom = exam.AvailableFrom,
                AvailableTo = exam.AvailableTo,
                AttemptLimit = exam.AttemptLimit,
                ShuffleQuestions = exam.ShuffleQuestions,
                ShuffleOptions = exam.ShuffleOptions,
                ShowResultsImmediately = exam.ShowResultsImmediately,
                ShowCorrectAnswers = exam.ShowCorrectAnswers,
                QuestionItemsJson = exam.QuestionItemsJson,
                IsPublished = exam.IsPublished,
                PublishedAt = exam.PublishedAt,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            };
        }

        public IQueryable<ExamODataResponse> GetOdataQueryable()
        {
            return _examRepository.GetQueryable().Select(exam => new ExamODataResponse
            {
                Id = exam.Id,
                SubjectId = exam.SubjectId,
                SubjectCode = exam.Subject.Code == null ? string.Empty : exam.Subject.Code,
                SubjectName = exam.Subject.Name == null ? string.Empty : exam.Subject.Name,
                CreatedBy = exam.CreatedBy,
                CreatorName = exam.Creator.FullName == null ? string.Empty : exam.Creator.FullName,
                Title = exam.Title,
                DurationMinutes = exam.DurationMinutes,
                Status = exam.Status,
                AvailableFrom = exam.AvailableFrom,
                AvailableTo = exam.AvailableTo,
                AttemptLimit = exam.AttemptLimit,
                IsPublished = exam.IsPublished,
                CreatedAt = exam.CreatedAt,
                UpdatedAt = exam.UpdatedAt
            });
        }
        

        public async Task<ServiceResult> UpdateAsync(int id, UpdateExamRequest request)
        {
            var exam = await _examRepository.GetDetailAsync(id);

            if (exam == null)
            {
                return ServiceResult.Fail("Không tìm thấy đề thi.");
            }

            // chỉ cho phép sửa khi đề thi còn ở trạng thái Draft
            if (!EditableStatuses.Contains(exam.Status))
            {
                return ServiceResult.Fail("Chỉ được cập nhật đề thi ở trạng thái Draft.");
            }

            // tái sử dụng validate đã có ở Create
            var validationError = await ValidateExamRequestAsync(
                request.SubjectId,
                request.DurationMinutes,
                request.AttemptLimit,
                request.AvailableFrom,
                request.AvailableTo);

            if (validationError != null)
            {
                return ServiceResult.Fail(validationError);
            }

            exam.SubjectId = request.SubjectId;
            exam.Title = request.Title.Trim();
            exam.Description = request.Description;
            exam.DurationMinutes = request.DurationMinutes;
            exam.AvailableFrom = request.AvailableFrom;
            exam.AvailableTo = request.AvailableTo;
            exam.AttemptLimit = request.AttemptLimit;
            exam.ShuffleQuestions = request.ShuffleQuestions;
            exam.ShuffleOptions = request.ShuffleOptions;
            exam.ShowResultsImmediately = request.ShowResultsImmediately;
            exam.ShowCorrectAnswers = request.ShowCorrectAnswers;
            exam.UpdatedAt = DateTime.Now;

            _examRepository.Update(exam);
            await _examRepository.SaveChangesAsync();

            return ServiceResult.Ok();
        }
    }
}
