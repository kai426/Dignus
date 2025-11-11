using Dignus.Candidate.Back.DTOs.Admin;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Data.Models.Core;
using Dignus.Data.Models.Enums;
using Dignus.Data.Models.Specialized;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dignus.Candidate.Back.Services.Admin;

/// <summary>
/// Service for managing Portuguese reading texts with questions
/// </summary>
public class PortugueseContentService : IPortugueseContentService
{
    private readonly IPortugueseReadingTextRepositoryNew _readingTextRepo;
    private readonly IQuestionTemplateRepository _questionTemplateRepo;
    private readonly ITestQuestionGroupRepository _questionGroupRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PortugueseContentService> _logger;

    public PortugueseContentService(
        IPortugueseReadingTextRepositoryNew readingTextRepo,
        IQuestionTemplateRepository questionTemplateRepo,
        ITestQuestionGroupRepository questionGroupRepo,
        IUnitOfWork unitOfWork,
        ILogger<PortugueseContentService> logger)
    {
        _readingTextRepo = readingTextRepo;
        _questionTemplateRepo = questionTemplateRepo;
        _questionGroupRepo = questionGroupRepo;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PortugueseContentDto> CreatePortugueseContentAsync(CreatePortugueseContentRequest request)
    {
        _logger.LogInformation("Creating Portuguese content: {Title}", request.Title);

        // 1. Get the active Portuguese question group
        var questionGroup = await _questionGroupRepo.GetActiveGroupByTestTypeAsync(TestType.Portuguese);
        if (questionGroup == null)
        {
            throw new InvalidOperationException("No active Portuguese question group found. Please ensure the question group seeder has run.");
        }

        // 2. Create the reading text
        var readingText = new PortugueseReadingText
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            AuthorName = request.AuthorName,
            SourceAttribution = request.SourceAttribution,
            DifficultyLevel = request.DifficultyLevel,
            EstimatedReadingTimeMinutes = request.EstimatedReadingTimeMinutes,
            WordCount = request.WordCount,
            IsActive = true,
            Version = 1,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _readingTextRepo.AddAsync(readingText);

        // 3. Create the questions
        var questionDtos = new List<PortugueseQuestionDto>();

        foreach (var questionRequest in request.Questions.OrderBy(q => q.QuestionOrder))
        {
            var questionTemplate = new QuestionTemplate
            {
                Id = Guid.NewGuid(),
                TestType = TestType.Portuguese,
                QuestionText = questionRequest.QuestionText,
                OptionsJson = null, // Video questions don't have options
                AllowMultipleAnswers = false,
                MaxAnswersAllowed = null,
                DifficultyLevel = request.DifficultyLevel, // Inherit from reading text
                PointValue = questionRequest.PointValue,
                EstimatedTimeSeconds = questionRequest.EstimatedTimeSeconds,
                CategoryTags = questionRequest.CategoryTags,
                IsActive = true,
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                TestQuestionGroupId = questionGroup.Id,
                GroupOrder = questionRequest.QuestionOrder
            };

            await _questionTemplateRepo.AddAsync(questionTemplate);

            // 4. Create the answer guide (for grading)
            if (!string.IsNullOrWhiteSpace(questionRequest.ExpectedAnswerGuideJson))
            {
                var questionAnswer = new QuestionAnswer
                {
                    QuestionTemplateId = questionTemplate.Id,
                    CorrectAnswer = null, // Video questions don't have a single correct answer
                    ExpectedAnswerGuideJson = questionRequest.ExpectedAnswerGuideJson,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                // Note: We need to add this through the context directly
                // since there's no separate repository for QuestionAnswer
                questionTemplate.Answer = questionAnswer;
            }

            questionDtos.Add(new PortugueseQuestionDto
            {
                QuestionId = questionTemplate.Id,
                QuestionText = questionTemplate.QuestionText,
                PointValue = questionTemplate.PointValue,
                EstimatedTimeSeconds = questionTemplate.EstimatedTimeSeconds,
                CategoryTags = questionTemplate.CategoryTags,
                QuestionOrder = questionRequest.QuestionOrder,
                IsActive = true
            });
        }

        // 5. Save all changes
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Created Portuguese content {ReadingTextId} with {QuestionCount} questions",
            readingText.Id, questionDtos.Count);

        return new PortugueseContentDto
        {
            ReadingTextId = readingText.Id,
            Title = readingText.Title,
            Content = readingText.Content,
            AuthorName = readingText.AuthorName,
            DifficultyLevel = readingText.DifficultyLevel,
            EstimatedReadingTimeMinutes = readingText.EstimatedReadingTimeMinutes,
            WordCount = readingText.WordCount,
            IsActive = readingText.IsActive,
            CreatedAt = readingText.CreatedAt,
            Questions = questionDtos
        };
    }

    public async Task<PortugueseContentDto> GetPortugueseContentAsync(Guid readingTextId)
    {
        var readingText = await _readingTextRepo.GetByIdAsync(readingTextId);
        if (readingText == null)
        {
            throw new NotFoundException("PortugueseReadingText", readingTextId);
        }

        // Get questions associated with this reading text's difficulty level
        var questions = await _questionTemplateRepo.GetByDifficultyAsync(
            TestType.Portuguese,
            readingText.DifficultyLevel);

        var questionDtos = questions
            .Where(q => q.GroupOrder.HasValue)
            .OrderBy(q => q.GroupOrder)
            .Select(q => new PortugueseQuestionDto
            {
                QuestionId = q.Id,
                QuestionText = q.QuestionText,
                PointValue = q.PointValue,
                EstimatedTimeSeconds = q.EstimatedTimeSeconds,
                CategoryTags = q.CategoryTags,
                QuestionOrder = q.GroupOrder ?? 0,
                IsActive = q.IsActive
            })
            .ToList();

        return new PortugueseContentDto
        {
            ReadingTextId = readingText.Id,
            Title = readingText.Title,
            Content = readingText.Content,
            AuthorName = readingText.AuthorName,
            DifficultyLevel = readingText.DifficultyLevel,
            EstimatedReadingTimeMinutes = readingText.EstimatedReadingTimeMinutes,
            WordCount = readingText.WordCount,
            IsActive = readingText.IsActive,
            CreatedAt = readingText.CreatedAt,
            Questions = questionDtos
        };
    }

    public async Task<List<PortugueseContentDto>> GetAllPortugueseContentAsync(
        bool includeInactive = false,
        int page = 1,
        int pageSize = 50)
    {
        var readingTexts = includeInactive
            ? (await _readingTextRepo.GetActiveTextsAsync()).ToList() // TODO: Need a GetAll method that includes inactive
            : await _readingTextRepo.GetActiveTextsAsync();

        var paginatedTexts = readingTexts
            .OrderByDescending(rt => rt.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new List<PortugueseContentDto>();

        foreach (var readingText in paginatedTexts)
        {
            var questions = await _questionTemplateRepo.GetByDifficultyAsync(
                TestType.Portuguese,
                readingText.DifficultyLevel);

            var questionDtos = questions
                .Where(q => q.GroupOrder.HasValue)
                .OrderBy(q => q.GroupOrder)
                .Select(q => new PortugueseQuestionDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    PointValue = q.PointValue,
                    EstimatedTimeSeconds = q.EstimatedTimeSeconds,
                    CategoryTags = q.CategoryTags,
                    QuestionOrder = q.GroupOrder ?? 0,
                    IsActive = q.IsActive
                })
                .ToList();

            result.Add(new PortugueseContentDto
            {
                ReadingTextId = readingText.Id,
                Title = readingText.Title,
                Content = readingText.Content,
                AuthorName = readingText.AuthorName,
                DifficultyLevel = readingText.DifficultyLevel,
                EstimatedReadingTimeMinutes = readingText.EstimatedReadingTimeMinutes,
                WordCount = readingText.WordCount,
                IsActive = readingText.IsActive,
                CreatedAt = readingText.CreatedAt,
                Questions = questionDtos
            });
        }

        return result;
    }

    public async Task DeactivatePortugueseContentAsync(Guid readingTextId)
    {
        var readingText = await _readingTextRepo.GetByIdAsync(readingTextId);
        if (readingText == null)
        {
            throw new NotFoundException("PortugueseReadingText", readingTextId);
        }

        readingText.IsActive = false;
        readingText.UpdatedAt = DateTimeOffset.UtcNow;

        await _readingTextRepo.UpdateAsync(readingText);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deactivated Portuguese content {ReadingTextId}", readingTextId);
    }
}
