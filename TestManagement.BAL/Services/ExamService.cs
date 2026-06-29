using System.Text.Json;
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
        private readonly IQuestionRepository _questionRepository;

        private static readonly string[] EditableStatuses = { "Draft" };
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public ExamService(
            IExamRepository repository,
            ISubjectRepository subjectRepository,
            IQuestionRepository questionRepository)
        {
            _examRepository = repository;
            _subjectRepository = subjectRepository;
            _questionRepository = questionRepository;
        }

        public IQueryable<ExamODataResponse> GetOdataQueryable()
        {
            return _examRepository.GetQueryable().Select(exam => new ExamODataResponse
            {
                Id = exam.Id,
                SubjectId = exam.SubjectId,
                SubjectCode = exam.Subject == null ? string.Empty : exam.Subject.Code,
                SubjectName = exam.Subject == null ? string.Empty : exam.Subject.Name,
                CreatedBy = exam.CreatedBy,
                CreatorName = exam.Creator == null ? string.Empty : exam.Creator.FullName,
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

        public async Task<ExamResponse?> GetByIdAsync(int id)
        {
            var exam = await _examRepository.GetDetailAsync(id);
            return exam == null ? null : MaptoResponse(exam);
        }

        public async Task<ServiceResult<ExamResponse>> CreateAsync(CreateExamRequest request, int? currentUserId)
        {
            if (currentUserId == null)
            {
                return ServiceResult<ExamResponse>.Fail("Không xác định được người tạo đề thi.");
            }

            var validationError = await ValidateExamRequestAsync(
                request.SubjectId,
                request.DurationMinutes,
                request.AttemptLimit,
                request.AvailableFrom,
                request.AvailableTo);

            if (validationError != null)
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

            var createdExam = await _examRepository.GetDetailAsync(exam.Id);
            if (createdExam == null)
            {
                return ServiceResult<ExamResponse>.Fail("Tạo đề thi thành công nhưng không tải được dữ liệu chi tiết.");
            }

            return ServiceResult<ExamResponse>.Ok(MaptoResponse(createdExam));
        }

        public async Task<ServiceResult> UpdateAsync(int id, UpdateExamRequest request)
        {
            var exam = await _examRepository.GetDetailAsync(id);
            if (exam == null)
            {
                return ServiceResult.Fail("Không tìm thấy đề thi.");
            }

            if (!EditableStatuses.Contains(exam.Status) || exam.IsPublished)
            {
                return ServiceResult.Fail("Chỉ được cập nhật đề thi ở trạng thái Draft.");
            }

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

        public async Task<ServiceResult> DeleteAsync(int id)
        {
            var exam = await _examRepository.GetDetailAsync(id);
            if (exam == null)
            {
                return ServiceResult.Fail("Không tìm thấy đề thi.");
            }

            if (!EditableStatuses.Contains(exam.Status) || exam.IsPublished)
            {
                return ServiceResult.Fail("Chỉ được xóa đề thi ở trạng thái Draft.");
            }

            exam.IsDeleted = true;
            exam.UpdatedAt = DateTime.Now;

            _examRepository.Update(exam);
            await _examRepository.SaveChangesAsync();

            return ServiceResult.Ok();
        }

        public async Task<ServiceResult<ExamQuestionsResponse>> GetQuestionsAsync(int examId)
        {
            var exam = await _examRepository.GetDetailAsync(examId);
            if (exam == null)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Không tìm thấy đề thi.");
            }

            var items = DeserializeQuestionItems(exam.QuestionItemsJson);
            if (!items.Any())
            {
                return ServiceResult<ExamQuestionsResponse>.Ok(MapToExamQuestionsResponse(exam, new List<Question>(), items));
            }

            var questions = await _questionRepository.GetActiveWithOptionsByIdsAsync(items.Select(x => x.QuestionId));
            if (questions.Count != items.Select(x => x.QuestionId).Distinct().Count())
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Một hoặc nhiều câu hỏi không tồn tại hoặc không còn hoạt động.");
            }

            return ServiceResult<ExamQuestionsResponse>.Ok(MapToExamQuestionsResponse(exam, questions, items));
        }

        public async Task<ServiceResult<ExamQuestionsResponse>> UpdateQuestionsAsync(int examId, UpdateExamQuestionsRequest request)
        {
            if (request == null)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Dữ liệu cập nhật câu hỏi không hợp lệ.");
            }

            var exam = await _examRepository.GetDetailAsync(examId);
            if (exam == null)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Không tìm thấy đề thi.");
            }

            if (!EditableStatuses.Contains(exam.Status) || exam.IsPublished)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Chỉ được cập nhật câu hỏi khi đề thi đang ở trạng thái Draft.");
            }

            var validationError = ValidateQuestionItems(request.Items);
            if (validationError != null)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail(validationError);
            }

            var questionIds = request.Items.Select(x => x.QuestionId).Distinct().ToList();
            var questions = await _questionRepository.GetActiveWithOptionsByIdsAsync(questionIds);

            if (questions.Count != questionIds.Count)
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Một hoặc nhiều câu hỏi không tồn tại hoặc không còn hoạt động.");
            }

            if (questions.Any(x => x.SubjectId != exam.SubjectId))
            {
                return ServiceResult<ExamQuestionsResponse>.Fail("Tất cả câu hỏi phải thuộc cùng môn học với đề thi.");
            }

            var normalizedItems = NormalizeQuestionItems(request.Items);
            exam.QuestionItemsJson = JsonSerializer.Serialize(normalizedItems, JsonOptions);
            exam.UpdatedAt = DateTime.Now;

            _examRepository.Update(exam);
            await _examRepository.SaveChangesAsync();

            return ServiceResult<ExamQuestionsResponse>.Ok(MapToExamQuestionsResponse(exam, questions, normalizedItems));
        }

        public async Task<ServiceResult<PublishExamResponse>> PublishAsync(int examId, int? currentUserId)
        {
            if (currentUserId == null)
            {
                return ServiceResult<PublishExamResponse>.Fail("Không xác định được người publish đề thi.");
            }

            var exam = await _examRepository.GetDetailAsync(examId);
            if (exam == null)
            {
                return ServiceResult<PublishExamResponse>.Fail("Không tìm thấy đề thi.");
            }

            if (!EditableStatuses.Contains(exam.Status) || exam.IsPublished)
            {
                return ServiceResult<PublishExamResponse>.Fail("Chỉ được publish đề thi ở trạng thái Draft.");
            }

            var items = DeserializeQuestionItems(exam.QuestionItemsJson);
            if (!items.Any())
            {
                return ServiceResult<PublishExamResponse>.Fail("Đề thi phải có ít nhất một câu hỏi trước khi publish.");
            }

            var questionIds = items.Select(x => x.QuestionId).Distinct().ToList();
            var questions = await _questionRepository.GetActiveWithOptionsByIdsAsync(questionIds);

            if (questions.Count != questionIds.Count)
            {
                return ServiceResult<PublishExamResponse>.Fail("Một hoặc nhiều câu hỏi không tồn tại hoặc không còn hoạt động.");
            }

            if (questions.Any(x => x.SubjectId != exam.SubjectId))
            {
                return ServiceResult<PublishExamResponse>.Fail("Tất cả câu hỏi phải thuộc cùng môn học với đề thi.");
            }

            var publishValidationError = ValidateQuestionsForPublish(questions);
            if (publishValidationError != null)
            {
                return ServiceResult<PublishExamResponse>.Fail(publishValidationError);
            }

            exam.PublishedSnapshotJson = BuildPublishedSnapshotJson(items, questions);
            exam.AnswerKeyJson = BuildAnswerKeyJson(items, questions);
            exam.Status = "Published";
            exam.IsPublished = true;
            exam.PublishedAt = DateTime.Now;
            exam.UpdatedAt = DateTime.Now;

            _examRepository.Update(exam);
            await _examRepository.SaveChangesAsync();

            return ServiceResult<PublishExamResponse>.Ok(new PublishExamResponse
            {
                ExamId = exam.Id,
                Status = exam.Status,
                IsPublished = exam.IsPublished,
                PublishedAt = exam.PublishedAt,
                QuestionCount = items.Count,
                TotalPoints = items.Sum(x => x.Points)
            });
        }

        private async Task<string?> ValidateExamRequestAsync(
            int subjectId,
            int durationMinutes,
            int attemptLimit,
            DateTime? availableFrom,
            DateTime? availableTo)
        {
            var subject = await _subjectRepository.GetByIdAsync(subjectId);
            if (subject == null || subject.Status != "Active")
            {
                return "Môn học không tìm thấy hoặc không tồn tại.";
            }

            if (durationMinutes <= 0)
            {
                return "Thời gian làm bài phải lớn hơn 0.";
            }

            if (attemptLimit <= 0)
            {
                return "Số lần làm bài phải lớn hơn 0.";
            }

            if (availableFrom.HasValue && availableTo.HasValue && availableFrom >= availableTo)
            {
                return "Thời gian bắt đầu phải nhỏ hơn thời gian kết thúc.";
            }

            return null;
        }

        private static string? ValidateQuestionItems(List<ExamQuestionItemRequest> items)
        {
            if (items == null || !items.Any())
            {
                return "Đề thi phải có ít nhất một câu hỏi.";
            }

            if (items.Any(x => x.QuestionId <= 0))
            {
                return "Mã câu hỏi không hợp lệ.";
            }

            if (items.Any(x => x.Points <= 0))
            {
                return "Điểm của mỗi câu hỏi phải lớn hơn 0.";
            }

            if (items.GroupBy(x => x.QuestionId).Any(x => x.Count() > 1))
            {
                return "Danh sách câu hỏi không được chứa câu hỏi trùng lặp.";
            }

            return null;
        }

        private static string? ValidateQuestionsForPublish(List<Question> questions)
        {
            foreach (var question in questions)
            {
                if (!question.Options.Any())
                {
                    return $"Câu hỏi {question.Id} phải có đáp án trước khi publish.";
                }

                var correctCount = question.Options.Count(option => option.IsCorrect);
                if (correctCount == 0)
                {
                    return $"Câu hỏi {question.Id} phải có ít nhất một đáp án đúng trước khi publish.";
                }

                if (question.QuestionType == "SingleChoice" && correctCount != 1)
                {
                    return $"Câu hỏi {question.Id} dạng SingleChoice phải có đúng một đáp án đúng.";
                }
            }

            return null;
        }

        private static List<ExamQuestionConfig> NormalizeQuestionItems(List<ExamQuestionItemRequest> items)
        {
            return items
                .Select((item, index) => new { Item = item, OriginalIndex = index })
                .OrderBy(x => x.Item.SortOrder <= 0 ? int.MaxValue : x.Item.SortOrder)
                .ThenBy(x => x.OriginalIndex)
                .Select((x, index) => new ExamQuestionConfig
                {
                    QuestionId = x.Item.QuestionId,
                    Points = x.Item.Points,
                    SortOrder = index + 1
                })
                .ToList();
        }

        private static List<ExamQuestionConfig> DeserializeQuestionItems(string? questionItemsJson)
        {
            if (string.IsNullOrWhiteSpace(questionItemsJson))
            {
                return new List<ExamQuestionConfig>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<ExamQuestionConfig>>(questionItemsJson, JsonOptions)
                    ?? new List<ExamQuestionConfig>();
            }
            catch (JsonException)
            {
                return new List<ExamQuestionConfig>();
            }
        }

        private static string BuildPublishedSnapshotJson(List<ExamQuestionConfig> items, List<Question> questions)
        {
            var questionMap = questions.ToDictionary(x => x.Id);
            var snapshot = new
            {
                questionCount = items.Count,
                totalPoints = items.Sum(x => x.Points),
                questions = items.OrderBy(x => x.SortOrder).Select(item =>
                {
                    var question = questionMap[item.QuestionId];
                    return new
                    {
                        questionId = question.Id,
                        sortOrder = item.SortOrder,
                        points = item.Points,
                        questionType = question.QuestionType,
                        content = question.Content,
                        difficulty = question.Difficulty,
                        options = question.Options.OrderBy(o => o.SortOrder).Select(option => new
                        {
                            optionId = option.Id,
                            label = option.Label,
                            content = option.Content,
                            sortOrder = option.SortOrder
                        })
                    };
                })
            };

            return JsonSerializer.Serialize(snapshot, JsonOptions);
        }

        private static string BuildAnswerKeyJson(List<ExamQuestionConfig> items, List<Question> questions)
        {
            var questionMap = questions.ToDictionary(x => x.Id);
            var answerKey = items.OrderBy(x => x.SortOrder).Select(item =>
            {
                var question = questionMap[item.QuestionId];
                return new
                {
                    questionId = question.Id,
                    questionType = question.QuestionType,
                    correctOptionIds = question.Options
                        .Where(option => option.IsCorrect)
                        .OrderBy(option => option.SortOrder)
                        .Select(option => option.Id)
                        .ToList(),
                    points = item.Points
                };
            });

            return JsonSerializer.Serialize(answerKey, JsonOptions);
        }

        private static ExamQuestionsResponse MapToExamQuestionsResponse(
            Exam exam,
            List<Question> questions,
            List<ExamQuestionConfig> items)
        {
            var questionMap = questions.ToDictionary(x => x.Id);
            var responseItems = items
                .OrderBy(x => x.SortOrder)
                .Where(x => questionMap.ContainsKey(x.QuestionId))
                .Select(x =>
                {
                    var question = questionMap[x.QuestionId];
                    return new ExamQuestionItemResponse
                    {
                        QuestionId = question.Id,
                        SortOrder = x.SortOrder,
                        Points = x.Points,
                        QuestionType = question.QuestionType,
                        Content = question.Content,
                        Difficulty = question.Difficulty,
                        Options = question.Options
                            .OrderBy(o => o.SortOrder)
                            .Select(o => new ExamQuestionOptionResponse
                            {
                                OptionId = o.Id,
                                Label = o.Label,
                                Content = o.Content,
                                SortOrder = o.SortOrder,
                                IsCorrect = o.IsCorrect
                            })
                            .ToList()
                    };
                })
                .ToList();

            return new ExamQuestionsResponse
            {
                ExamId = exam.Id,
                Status = exam.Status,
                IsPublished = exam.IsPublished,
                QuestionCount = responseItems.Count,
                TotalPoints = responseItems.Sum(x => x.Points),
                Items = responseItems
            };
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

        private class ExamQuestionConfig
        {
            public int QuestionId { get; set; }
            public int SortOrder { get; set; }
            public decimal Points { get; set; }
        }
    }
}
