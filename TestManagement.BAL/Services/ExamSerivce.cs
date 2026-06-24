using System;
using System.Collections.Generic;
using System.Linq;
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

        public ExamService(IExamRepository examRepository)
        {
            _examRepository = examRepository;
        }

        public IQueryable<ExamODataResponse> GetOdataQueryable()
        {
            return _examRepository.GetQueryable()
                .Select(e => new ExamODataResponse
                {
                    Id = e.Id,
                    SubjectId = e.SubjectId,
                    SubjectCode = e.Subject != null ? e.Subject.Code : string.Empty,
                    SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
                    CreatedBy = e.CreatedBy,
                    CreatorName = e.Creator != null ? e.Creator.FullName : string.Empty,
                    Title = e.Title,
                    DurationMinutes = e.DurationMinutes,
                    Status = e.Status,
                    AvailableFrom = e.AvailableFrom,
                    AvailableTo = e.AvailableTo,
                    AttemptLimit = e.AttemptLimit,
                    IsPublished = e.IsPublished,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt
                });
        }

        public async Task<ExamResponse> GetByIdAsync(int id)
        {
            var e = await _examRepository.GetDetailAsync(id);
            if (e == null)
            {
                // Vì Interface yêu cầu trả về ExamResponse (không cho phép null ?), 
                // Ta trả về một object trống hoặc có thể throw KeyNotFoundException tùy logic dự án.
                return new ExamResponse(); 
            }

            return new ExamResponse
            {
                Id = e.Id,
                SubjectId = e.SubjectId,
                SubjectCode = e.Subject != null ? e.Subject.Code : string.Empty,
                SubjectName = e.Subject != null ? e.Subject.Name : string.Empty,
                CreatedBy = e.CreatedBy,
                CreatorName = e.Creator != null ? e.Creator.FullName : string.Empty,
                Title = e.Title,
                Description = e.Description,
                DurationMinutes = e.DurationMinutes,
                Status = e.Status,
                AvailableFrom = e.AvailableFrom,
                AvailableTo = e.AvailableTo,
                AttemptLimit = e.AttemptLimit,
                ShuffleQuestions = e.ShuffleQuestions,
                ShuffleOptions = e.ShuffleOptions,
                ShowResultsImmediately = e.ShowResultsImmediately,
                ShowCorrectAnswers = e.ShowCorrectAnswers,
                QuestionItemsJson = e.QuestionItemsJson,
                IsPublished = e.IsPublished,
                PublishedAt = e.PublishedAt,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            };
        }

        public async Task<ServiceResult<ExamResponse>> CreateAsync(CreateExamRequest request, int? currentUserId)
        {
            try
            {
                var exam = new Exam
                {
                    SubjectId = request.SubjectId,
                    CreatedBy = currentUserId ?? 0,
                    Title = request.Title,
                    Description = request.Description,
                    DurationMinutes = request.DurationMinutes,
                    AvailableFrom = request.AvailableFrom,
                    AvailableTo = request.AvailableTo,
                    AttemptLimit = request.AttemptLimit,
                    ShuffleQuestions = request.ShuffleQuestions,
                    ShuffleOptions = request.ShuffleOptions,
                    ShowResultsImmediately = request.ShowResultsImmediately,
                    ShowCorrectAnswers = request.ShowCorrectAnswers,
                    Status = "Draft",
                    IsPublished = false,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _examRepository.AddAsync(exam);

                var response = await GetByIdAsync(exam.Id);
                
                // Khởi tạo ServiceResult thành công bằng Constructor thay vì hàm Static
                return new ServiceResult<ExamResponse>
                {
                    Success = true,
                    Data = response
                };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ExamResponse>
                {
                    Success = false,
                    Error = $"Lỗi khi tạo đề thi: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateExamRequest request)
        {
            try
            {
                var exam = await _examRepository.GetByIdAsync(id);
                if (exam == null || exam.IsDeleted) 
                {
                    return new ServiceResult { Success = false, Error = "Không tìm thấy bài kiểm tra này." };
                }
                
                if (exam.IsPublished) 
                {
                    return new ServiceResult { Success = false, Error = "Không thể chỉnh sửa đề thi đã được xuất bản." };
                }

                exam.SubjectId = request.SubjectId;
                exam.Title = request.Title;
                exam.Description = request.Description;
                exam.DurationMinutes = request.DurationMinutes;
                exam.AvailableFrom = request.AvailableFrom;
                exam.AvailableTo = request.AvailableTo;
                exam.AttemptLimit = request.AttemptLimit;
                exam.ShuffleQuestions = request.ShuffleQuestions;
                exam.ShuffleOptions = request.ShuffleOptions;
                exam.ShowResultsImmediately = request.ShowResultsImmediately;
                exam.ShowCorrectAnswers = request.ShowCorrectAnswers;
                exam.UpdatedAt = DateTime.UtcNow;

                // Thay UpdateAsync bằng phương thức Update (đồng bộ) có sẵn của Base Repository trong prn232
                _examRepository.Update(exam); 
                
                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Error = $"Lỗi khi cập nhật đề thi: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            try
            {
                var exam = await _examRepository.GetByIdAsync(id);
                if (exam == null || exam.IsDeleted) 
                {
                    return new ServiceResult { Success = false, Error = "Không tìm thấy đề thi hoặc đề thi đã bị xóa trước đó." };
                }

                exam.IsDeleted = true;
                exam.UpdatedAt = DateTime.UtcNow;

                // Sử dụng phương thức Update (đồng bộ) để lưu trạng thái IsDeleted = true
                _examRepository.Update(exam); 
                
                return new ServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Error = $"Lỗi khi xóa đề thi: {ex.Message}" };
            }
        }
    }
}