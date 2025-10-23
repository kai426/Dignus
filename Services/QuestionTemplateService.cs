using AutoMapper;
using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Core;
using Dignus.Data.Models.Enums;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dignus.Candidate.Back.Services;

/// <summary>
/// Service for managing question templates (admin operations)
/// ⚠️ SECURITY: All methods should be protected by admin/recruiter authorization
/// </summary>
public class QuestionTemplateService : IQuestionTemplateService
{
    private readonly IQuestionTemplateRepository _questionTemplateRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionTemplateService> _logger;

    public QuestionTemplateService(
        IQuestionTemplateRepository questionTemplateRepo,
        IMapper mapper,
        ILogger<QuestionTemplateService> logger)
    {
        _questionTemplateRepo = questionTemplateRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<QuestionTemplateDto> CreateQuestionAsync(CreateQuestionTemplateRequest request)
    {
        _logger.LogInformation("Creating new question template for test type {TestType}", request.TestType);

        // Create question template entity
        var questionTemplate = new QuestionTemplate
        {
            Id = Guid.NewGuid(),
            TestType = request.TestType,
            QuestionText = request.QuestionText,
            OptionsJson = request.OptionsJson,
            AllowMultipleAnswers = request.AllowMultipleAnswers,
            MaxAnswersAllowed = request.MaxAnswersAllowed,
            DifficultyLevel = request.DifficultyLevel,
            PointValue = request.PointValue,
            EstimatedTimeSeconds = request.EstimatedTimeSeconds,
            CategoryTags = request.CategoryTags,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedByRecruiterId = request.CreatedByRecruiterId
        };

        // Create answer entity if provided
        if (!string.IsNullOrEmpty(request.CorrectAnswer) ||
            !string.IsNullOrEmpty(request.ExpectedAnswerGuideJson))
        {
            questionTemplate.Answer = new QuestionAnswer
            {
                QuestionTemplateId = questionTemplate.Id,
                CorrectAnswer = request.CorrectAnswer,
                ExpectedAnswerGuideJson = request.ExpectedAnswerGuideJson,
                AcceptableVariationsJson = request.AcceptableVariationsJson,
                UpdatedAt = DateTimeOffset.UtcNow,
                UpdatedByRecruiterId = request.CreatedByRecruiterId
            };
        }

        await _questionTemplateRepo.AddAsync(questionTemplate);
        await _questionTemplateRepo.SaveAsync();

        _logger.LogInformation("Created question template {QuestionId}", questionTemplate.Id);

        // Fetch the created question and map to DTO
        var created = await _questionTemplateRepo.GetByIdWithAnswerAsync(questionTemplate.Id);
        return MapToDto(created!);
    }

    public async Task<QuestionTemplateDto> UpdateQuestionAsync(UpdateQuestionTemplateRequest request)
    {
        var questionTemplate = await _questionTemplateRepo.GetByIdWithAnswerAsync(request.Id);

        if (questionTemplate == null)
        {
            throw new NotFoundException("QuestionTemplate", request.Id);
        }

        _logger.LogInformation("Updating question template {QuestionId}", request.Id);

        // Update question fields (only if provided)
        if (!string.IsNullOrEmpty(request.QuestionText))
            questionTemplate.QuestionText = request.QuestionText;

        if (request.OptionsJson != null)
            questionTemplate.OptionsJson = request.OptionsJson;

        if (request.AllowMultipleAnswers.HasValue)
            questionTemplate.AllowMultipleAnswers = request.AllowMultipleAnswers.Value;

        if (request.MaxAnswersAllowed.HasValue)
            questionTemplate.MaxAnswersAllowed = request.MaxAnswersAllowed;

        if (!string.IsNullOrEmpty(request.DifficultyLevel))
            questionTemplate.DifficultyLevel = request.DifficultyLevel;

        if (request.PointValue.HasValue)
            questionTemplate.PointValue = request.PointValue.Value;

        if (request.EstimatedTimeSeconds.HasValue)
            questionTemplate.EstimatedTimeSeconds = request.EstimatedTimeSeconds;

        if (request.CategoryTags != null)
            questionTemplate.CategoryTags = request.CategoryTags;

        // Update answer if provided
        if (!string.IsNullOrEmpty(request.CorrectAnswer) ||
            !string.IsNullOrEmpty(request.ExpectedAnswerGuideJson))
        {
            if (questionTemplate.Answer == null)
            {
                // Create new answer
                questionTemplate.Answer = new QuestionAnswer
                {
                    QuestionTemplateId = questionTemplate.Id,
                    CorrectAnswer = request.CorrectAnswer,
                    ExpectedAnswerGuideJson = request.ExpectedAnswerGuideJson,
                    AcceptableVariationsJson = request.AcceptableVariationsJson,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    UpdatedByRecruiterId = request.UpdatedByRecruiterId
                };
            }
            else
            {
                // Update existing answer
                if (!string.IsNullOrEmpty(request.CorrectAnswer))
                    questionTemplate.Answer.CorrectAnswer = request.CorrectAnswer;

                if (!string.IsNullOrEmpty(request.ExpectedAnswerGuideJson))
                    questionTemplate.Answer.ExpectedAnswerGuideJson = request.ExpectedAnswerGuideJson;

                if (!string.IsNullOrEmpty(request.AcceptableVariationsJson))
                    questionTemplate.Answer.AcceptableVariationsJson = request.AcceptableVariationsJson;

                questionTemplate.Answer.UpdatedAt = DateTimeOffset.UtcNow;
                questionTemplate.Answer.UpdatedByRecruiterId = request.UpdatedByRecruiterId;
            }
        }

        await _questionTemplateRepo.UpdateAsync(questionTemplate);
        await _questionTemplateRepo.SaveAsync();

        _logger.LogInformation("Updated question template {QuestionId} to version {Version}",
            questionTemplate.Id, questionTemplate.Version);

        // Fetch updated question
        var updated = await _questionTemplateRepo.GetByIdWithAnswerAsync(request.Id);
        return MapToDto(updated!);
    }

    public async Task<QuestionTemplateDto> GetQuestionByIdAsync(Guid id)
    {
        var question = await _questionTemplateRepo.GetByIdWithAnswerAsync(id);

        if (question == null)
        {
            throw new NotFoundException("QuestionTemplate", id);
        }

        return MapToDto(question);
    }

    public async Task<QuestionTemplateDetailDto> GetQuestionWithAnswerAsync(Guid id)
    {
        var question = await _questionTemplateRepo.GetByIdWithAnswerAsync(id);

        if (question == null)
        {
            throw new NotFoundException("QuestionTemplate", id);
        }

        return MapToDetailDto(question);
    }

    public async Task<PaginatedResult<QuestionTemplateDto>> GetQuestionsByTestTypeAsync(
        TestType testType,
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var allQuestions = await _questionTemplateRepo.GetByTestTypeAsync(testType, includeInactive);

        var totalCount = allQuestions.Count;
        var items = allQuestions
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Fetch answers for the paginated items (to determine HasAnswer)
        var questionIds = items.Select(q => q.Id).ToList();
        var questionsWithAnswers = new List<QuestionTemplate>();
        foreach (var id in questionIds)
        {
            var q = await _questionTemplateRepo.GetByIdWithAnswerAsync(id);
            if (q != null) questionsWithAnswers.Add(q);
        }

        var dtos = questionsWithAnswers.Select(MapToDto).ToList();

        return new PaginatedResult<QuestionTemplateDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<QuestionTemplateDto>> GetQuestionsByDifficultyAsync(TestType testType, string difficulty)
    {
        var questions = await _questionTemplateRepo.GetByDifficultyAsync(testType, difficulty);

        // Fetch answers for each question
        var questionsWithAnswers = new List<QuestionTemplate>();
        foreach (var q in questions)
        {
            var withAnswer = await _questionTemplateRepo.GetByIdWithAnswerAsync(q.Id);
            if (withAnswer != null) questionsWithAnswers.Add(withAnswer);
        }

        return questionsWithAnswers.Select(MapToDto).ToList();
    }

    public async Task<List<QuestionTemplateDto>> GetQuestionsByCategoryAsync(TestType testType, string category)
    {
        var questions = await _questionTemplateRepo.GetByCategoryAsync(testType, category);

        // Fetch answers for each question
        var questionsWithAnswers = new List<QuestionTemplate>();
        foreach (var q in questions)
        {
            var withAnswer = await _questionTemplateRepo.GetByIdWithAnswerAsync(q.Id);
            if (withAnswer != null) questionsWithAnswers.Add(withAnswer);
        }

        return questionsWithAnswers.Select(MapToDto).ToList();
    }

    public async Task DeactivateQuestionAsync(Guid id)
    {
        var question = await _questionTemplateRepo.GetByIdAsync(id);

        if (question == null)
        {
            throw new NotFoundException("QuestionTemplate", id);
        }

        _logger.LogInformation("Deactivating question template {QuestionId}", id);

        await _questionTemplateRepo.DeactivateAsync(id);
        await _questionTemplateRepo.SaveAsync();
    }

    public async Task ReactivateQuestionAsync(Guid id)
    {
        var question = await _questionTemplateRepo.GetByIdAsync(id);

        if (question == null)
        {
            throw new NotFoundException("QuestionTemplate", id);
        }

        _logger.LogInformation("Reactivating question template {QuestionId}", id);

        question.IsActive = true;
        question.UpdatedAt = DateTimeOffset.UtcNow;

        await _questionTemplateRepo.UpdateAsync(question);
        await _questionTemplateRepo.SaveAsync();
    }

    public async Task<QuestionBankStatistics> GetQuestionBankStatisticsAsync(TestType? testType = null)
    {
        var stats = new QuestionBankStatistics();

        // Get questions for each test type or specific test type
        var testTypes = testType.HasValue
            ? new[] { testType.Value }
            : Enum.GetValues<TestType>();

        foreach (var type in testTypes)
        {
            var questions = await _questionTemplateRepo.GetByTestTypeAsync(type, includeInactive: true);

            stats.TotalQuestions += questions.Count;
            stats.ActiveQuestions += questions.Count(q => q.IsActive);
            stats.InactiveQuestions += questions.Count(q => !q.IsActive);

            // Count by test type
            if (!stats.QuestionsByTestType.ContainsKey(type))
                stats.QuestionsByTestType[type] = 0;
            stats.QuestionsByTestType[type] += questions.Count;

            // Count by difficulty
            foreach (var question in questions.Where(q => q.IsActive))
            {
                var difficulty = question.DifficultyLevel ?? "unknown";
                if (!stats.QuestionsByDifficulty.ContainsKey(difficulty))
                    stats.QuestionsByDifficulty[difficulty] = 0;
                stats.QuestionsByDifficulty[difficulty]++;

                // Count by category
                if (question.CategoryTags != null && question.CategoryTags.Length > 0)
                {
                    foreach (var category in question.CategoryTags)
                    {
                        if (!string.IsNullOrWhiteSpace(category))
                        {
                            if (!stats.QuestionsByCategory.ContainsKey(category))
                                stats.QuestionsByCategory[category] = 0;
                            stats.QuestionsByCategory[category]++;
                        }
                    }
                }
            }
        }

        return stats;
    }

    #region Helper Methods

    /// <summary>
    /// Map QuestionTemplate to DTO (without exposing answer details)
    /// </summary>
    private QuestionTemplateDto MapToDto(QuestionTemplate question)
    {
        return new QuestionTemplateDto
        {
            Id = question.Id,
            TestType = question.TestType,
            QuestionText = question.QuestionText,
            OptionsJson = question.OptionsJson,
            AllowMultipleAnswers = question.AllowMultipleAnswers,
            MaxAnswersAllowed = question.MaxAnswersAllowed,
            DifficultyLevel = question.DifficultyLevel ?? "medium",
            PointValue = question.PointValue,
            EstimatedTimeSeconds = question.EstimatedTimeSeconds,
            CategoryTags = question.CategoryTags,
            IsActive = question.IsActive,
            Version = question.Version,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            HasAnswer = question.Answer != null,
            AnswerLastUpdated = question.Answer?.UpdatedAt
        };
    }

    /// <summary>
    /// Map QuestionTemplate to detail DTO (includes answer)
    /// ⚠️ SECURITY: Only use for admin endpoints
    /// </summary>
    private QuestionTemplateDetailDto MapToDetailDto(QuestionTemplate question)
    {
        return new QuestionTemplateDetailDto
        {
            Id = question.Id,
            TestType = question.TestType,
            QuestionText = question.QuestionText,
            OptionsJson = question.OptionsJson,
            AllowMultipleAnswers = question.AllowMultipleAnswers,
            MaxAnswersAllowed = question.MaxAnswersAllowed,
            DifficultyLevel = question.DifficultyLevel ?? "medium",
            PointValue = question.PointValue,
            EstimatedTimeSeconds = question.EstimatedTimeSeconds,
            CategoryTags = question.CategoryTags,
            IsActive = question.IsActive,
            Version = question.Version,
            CreatedAt = question.CreatedAt,
            UpdatedAt = question.UpdatedAt,
            CreatedByRecruiterId = question.CreatedByRecruiterId,
            Answer = question.Answer != null ? new QuestionAnswerDto
            {
                CorrectAnswer = question.Answer.CorrectAnswer,
                ExpectedAnswerGuideJson = question.Answer.ExpectedAnswerGuideJson,
                AcceptableVariationsJson = question.Answer.AcceptableVariationsJson,
                UpdatedAt = question.Answer.UpdatedAt,
                UpdatedByRecruiterId = question.Answer.UpdatedByRecruiterId
            } : null
        };
    }

    #endregion
}
