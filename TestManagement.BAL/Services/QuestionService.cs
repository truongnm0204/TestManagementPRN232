using TestManagement.BAL.DTOs.Common;
using TestManagement.BAL.DTOs.Questions;
using TestManagement.BAL.Services.Interfaces;
using TestManagement.DAL.Models;
using TestManagement.DAL.Repositories.Interfaces;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TestManagement.BAL.Services;

public class QuestionService : IQuestionService
{
    private static readonly string[] ValidDifficulties = { "Easy", "Medium", "Hard" };
    private static readonly string[] ValidStatuses = { "Active", "Inactive" };
    private readonly IQuestionRepository _questionRepository;
    private readonly ISubjectRepository _subjectRepository;
    private readonly ITopicRepository _topicRepository;
    private static readonly string[] ValidQuestionTypes = { "SingleChoice", "MultipleChoice", "TrueFalse" };
    private static readonly string[] ValidSourceTypes = { "Manual", "Import" };

    public QuestionService(IQuestionRepository questionRepository, ISubjectRepository subjectRepository, ITopicRepository topicRepository)
    {
        _questionRepository = questionRepository;
        _subjectRepository = subjectRepository;
        _topicRepository = topicRepository;
    }

    public IQueryable<QuestionODataResponse> GetODataQueryable()
    {
        return _questionRepository.GetQueryable().Select(question => new QuestionODataResponse
        {
            Id = question.Id,
            SubjectId = question.SubjectId,
            TopicId = question.TopicId,
            QuestionType = question.QuestionType,
            SourceType = question.SourceType,
            SubjectCode = question.Subject == null ? string.Empty : question.Subject.Code,
            SubjectName = question.Subject == null ? string.Empty : question.Subject.Name,
            Content = question.Content,
            Difficulty = question.Difficulty,
            Status = question.Status,
            CreatedAt = question.CreatedAt,
            CreatedByUserId = question.CreatedByUserId,
            Options = question.Options
                .OrderBy(o => o.SortOrder)
                .Select(o => new QuestionOptionResponse
                {
                    Id = o.Id,
                    Label = o.Label,
                    Content = o.Content,
                    IsCorrect = o.IsCorrect,
                    SortOrder = o.SortOrder
                }).ToList()
        });
    }

    public async Task<QuestionResponse?> GetByIdAsync(int id)
    {
        var question = await _questionRepository.GetDetailAsync(id);
        return question == null ? null : MapToResponse(question);
    }

    public async Task<ServiceResult<QuestionResponse>> CreateAsync(CreateQuestionRequest request, int? currentUserId)
    {
        var validationError = await ValidateQuestionRequestAsync(
            request.SubjectId,
            request.QuestionType,
            request.SourceType,
            request.TopicId,
            request.Difficulty,
            request.Status,
            request.Options);

        if (validationError != null)
        {
            return ServiceResult<QuestionResponse>.Fail(validationError);
        }

        var question = new Question
        {
            SubjectId = request.SubjectId,
            TopicId = request.TopicId,
            QuestionType = request.QuestionType,
            SourceType = request.SourceType,
            Content = request.Content,
            Explanation = request.Explanation,
            Difficulty = request.Difficulty,
            Status = request.Status,
            CreatedAt = DateTime.Now,
            CreatedByUserId = currentUserId,
            Options = MapOptions(request.Options)
        };

        await _questionRepository.AddAsync(question);
        await _questionRepository.SaveChangesAsync();

        var createdQuestion = await _questionRepository.GetDetailAsync(question.Id);
        return ServiceResult<QuestionResponse>.Ok(MapToResponse(createdQuestion ?? question));
    }

    public async Task<ServiceResult> UpdateAsync(int id, UpdateQuestionRequest request)
    {
        var question = await _questionRepository.GetDetailAsync(id);

        if (question == null)
        {
            return ServiceResult.Fail("Không tìm thấy câu hỏi.");
        }

        var validationError = await ValidateQuestionRequestAsync(
            request.SubjectId,
            request.QuestionType,
            request.SourceType,
            request.TopicId,
            request.Difficulty,
            request.Status,
            request.Options);

        if (validationError != null)
        {
            return ServiceResult.Fail(validationError);
        }

        question.SubjectId = request.SubjectId;
        question.TopicId = request.TopicId;
        question.QuestionType = request.QuestionType;
        question.SourceType = request.SourceType;
        question.Content = request.Content;
        question.Explanation = request.Explanation;
        question.Difficulty = request.Difficulty;
        question.Status = request.Status;
        question.UpdatedAt = DateTime.Now;

        await _questionRepository.ReplaceOptionsAsync(question, MapOptions(request.Options, question.Id));
        _questionRepository.Update(question);
        await _questionRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        var question = await _questionRepository.GetDetailAsync(id);

        if (question == null)
        {
            return ServiceResult.Fail("Không tìm thấy câu hỏi.");
        }

        question.IsDeleted = true;
        question.UpdatedAt = DateTime.Now;

        _questionRepository.Update(question);
        await _questionRepository.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    private async Task<string?> ValidateQuestionRequestAsync(
        int subjectId,
        string questionType,
        string sourceType,
        int? topicId,
        string difficulty,
        string status,
        List<QuestionOptionRequest> options)
    {

        var correctOptionCount = options.Count(x => x.IsCorrect);
        if (!ValidDifficulties.Contains(difficulty))
        {
            return "Độ khó không hợp lệ.";
        }

        if (!ValidStatuses.Contains(status))
        {
            return "Trạng thái câu hỏi không hợp lệ.";
        }

        var subject = await _subjectRepository.GetActiveByIdAsync(subjectId);

        if (subject == null || subject.Status != "Active")
        {
            return "Môn học không tồn tại hoặc không active.";
        }

        if (topicId.HasValue)
        {
            var topic = await _topicRepository.GetActiveByIdAsync(topicId.Value);
            if (topic == null || topic.SubjectId != subjectId)
            {
                return "Chủ đề không tồn tại hoặc không thuộc môn học đã chọn.";
            }
        }

        var optionValidationError = ValidateOptions(options);
        if (optionValidationError != null)
        {
            return optionValidationError;
        }
        if (string.IsNullOrEmpty(questionType) || !ValidQuestionTypes.Contains(questionType))
        {
            return "Loại câu hỏi không hợp lệ.";
        }
        if(!ValidSourceTypes.Contains(sourceType))
        {
            return "Loại nguồn gốc câu hỏi không hợp lệ.";
        }
        if(questionType.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase) && correctOptionCount != 1)
        {
            return "Câu hỏi Single Choice chỉ được phép có một đáp án đúng.";
        }
        if (questionType.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase) && correctOptionCount < 2)
        {
            return "Câu hỏi Multiple Choice phải có ít nhất hai đáp án đúng.";
        }
        if(questionType.Equals("TrueFalse", StringComparison.OrdinalIgnoreCase))
        {
            if(options.Count != 2)
            {
                return "Câu hỏi True/False phải có đúng 2 đáp án.";
            }
            if(options.Count(x=> x.IsCorrect) != 1) { 
                return "Câu hỏi True/False phải có đúng 1 đáp án đúng.";
            }
        }
        return null;
    }

    private static string? ValidateOptions(List<QuestionOptionRequest> options)
    {
        if (options.Count < 2)
        {
            return "Câu hỏi phải có ít nhất 2 đáp án.";
        }

        if (options.Count(x => x.IsCorrect) == 0)
        {
            return "Câu hỏi phải có ít nhất 1 đáp án đúng.";
        }

        if (options.Any(x => string.IsNullOrWhiteSpace(x.Label) || string.IsNullOrWhiteSpace(x.Content)))
        {
            return "Nhãn và nội dung đáp án là bắt buộc.";
        }

        var hasDuplicateLabel = options
            .GroupBy(x => x.Label.Trim().ToUpperInvariant())
            .Any(group => group.Count() > 1);

        return hasDuplicateLabel ? "Nhãn đáp án không được trùng." : null;
    }

    private static List<QuestionOption> MapOptions(IEnumerable<QuestionOptionRequest> options, int questionId = 0)
    {
        return options
            .Select((option, index) => new QuestionOption
            {
                QuestionId = questionId,
                Label = option.Label.Trim().ToUpperInvariant(),
                Content = option.Content,
                IsCorrect = option.IsCorrect,
                SortOrder = option.SortOrder > 0 ? option.SortOrder : index + 1
            })
            .ToList();
    }

    private static QuestionResponse MapToResponse(Question question)
    {
        return new QuestionResponse
        {
            Id = question.Id,
            SubjectId = question.SubjectId,
            TopicId = question.TopicId,
            QuestionType = question.QuestionType,
            SourceType = question.SourceType,
            SubjectCode = question.Subject?.Code ?? string.Empty,
            SubjectName = question.Subject?.Name ?? string.Empty,
            Content = question.Content,
            Explanation = question.Explanation,
            Difficulty = question.Difficulty,
            Status = question.Status,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            Options = question.Options
                .OrderBy(option => option.SortOrder)
                .Select(option => new QuestionOptionResponse
                {
                    Id = option.Id,
                    Label = option.Label,
                    Content = option.Content,
                    IsCorrect = option.IsCorrect,
                    SortOrder = option.SortOrder
                })
                .ToList()
        };
    }
}
