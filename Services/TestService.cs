using AutoMapper;
using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Core;
using Dignus.Data.Models.Enums;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TestStatus = Dignus.Data.Models.TestStatus;

namespace Dignus.Candidate.Back.Services;

/// <summary>
/// Unified test service for all test types
/// Handles test creation, snapshotting, submission, and auto-grading
/// </summary>
public class TestService : ITestService
{
    private readonly ITestInstanceRepository _testInstanceRepo;
    private readonly IQuestionTemplateRepository _questionTemplateRepo;
    private readonly ITestQuestionSnapshotRepository _snapshotRepo;
    private readonly ITestQuestionResponseRepository _questionResponseRepo;
    private readonly ITestVideoResponseRepository _videoResponseRepo;
    private readonly IPortugueseReadingTextRepositoryNew _portugueseReadingTextRepo;
    private readonly ITestQuestionGroupRepository _questionGroupRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<TestService> _logger;

    // Test configuration constants
    private static readonly Dictionary<TestType, int> QuestionCounts = new()
    {
        { TestType.Portuguese, 3 },     // 3 video questions (reading text handled separately)
        { TestType.Math, 2 },            // 2 video questions
        { TestType.Interview, 5 },       // 5 video interview questions
        { TestType.Psychology, 52 },     // 52 personality assessment questions - all delivered in order
        { TestType.VisualRetention, 15 }
    };

    private static readonly Dictionary<TestType, int?> TimeLimitsSeconds = new()
    {
        { TestType.Portuguese, null }, // No time limit (video-based)
        { TestType.Math, null },       // No time limit (video-based)
        { TestType.Psychology, 3600 }, // 60 minutes
        { TestType.VisualRetention, 1200 } // 20 minutes
    };

    public TestService(
        ITestInstanceRepository testInstanceRepo,
        IQuestionTemplateRepository questionTemplateRepo,
        ITestQuestionSnapshotRepository snapshotRepo,
        ITestQuestionResponseRepository questionResponseRepo,
        ITestVideoResponseRepository videoResponseRepo,
        IPortugueseReadingTextRepositoryNew portugueseReadingTextRepo,
        ITestQuestionGroupRepository questionGroupRepo,
        IMapper mapper,
        ILogger<TestService> logger)
    {
        _testInstanceRepo = testInstanceRepo;
        _questionTemplateRepo = questionTemplateRepo;
        _snapshotRepo = snapshotRepo;
        _questionResponseRepo = questionResponseRepo;
        _videoResponseRepo = videoResponseRepo;
        _portugueseReadingTextRepo = portugueseReadingTextRepo;
        _questionGroupRepo = questionGroupRepo;
        _mapper = mapper;
        _logger = logger;
    }

    #region Public Methods

    public async Task<TestInstanceDto> CreateTestAsync(CreateTestInstanceRequest request)
    {
        _logger.LogInformation("Creating {TestType} test for candidate {CandidateId}",
            request.TestType, request.CandidateId);

        // 1. Check if candidate can start this test type
        if (!await CanStartTestAsync(request.CandidateId, request.TestType))
        {
            throw new InvalidOperationException(
                $"Candidate {request.CandidateId} already has an active {request.TestType} test");
        }

        // 2. Create test instance
        var testInstance = _mapper.Map<TestInstance>(request);
        testInstance.Id = Guid.NewGuid();
        testInstance.Status = TestStatus.NotStarted;
        testInstance.CreatedAt = DateTimeOffset.UtcNow;

        // 3. Initialize test-type-specific data
        await InitializeTestTypeSpecificsAsync(testInstance, request.DifficultyLevel);

        // 4. Save test instance FIRST (before snapshots, so foreign keys work)
        await _testInstanceRepo.AddAsync(testInstance);
        await _testInstanceRepo.SaveAsync();

        // 5. Snapshot questions from template bank (after test is saved)
        await SnapshotQuestionsForTestAsync(testInstance, request.DifficultyLevel);

        _logger.LogInformation("Created test {TestId} for candidate {CandidateId}",
            testInstance.Id, request.CandidateId);

        // 6. Re-fetch the test to ensure all data is persisted and navigation properties are loaded
        // This ensures InMemory database consistency across requests
        var savedTest = await _testInstanceRepo.GetByIdFullAsync(testInstance.Id);
        if (savedTest == null)
        {
            _logger.LogError("Failed to retrieve just-created test {TestId}", testInstance.Id);
            throw new InvalidOperationException($"Test {testInstance.Id} was created but could not be retrieved");
        }

        return _mapper.Map<TestInstanceDto>(savedTest);
    }

    public async Task<TestInstanceDto> GetTestByIdAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdFullAsync(testId);

        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        // IDOR Protection: Validate candidate owns this test
        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException(
                $"Test {testId} does not belong to candidate {candidateId}");
        }

        return _mapper.Map<TestInstanceDto>(test);
    }

    public async Task<List<TestInstanceDto>> GetCandidateTestsAsync(Guid candidateId, TestType? filterByType = null)
    {
        var tests = filterByType.HasValue
            ? await _testInstanceRepo.GetByCandidateAndTypeAsync(candidateId, filterByType.Value)
            : await _testInstanceRepo.GetByCandidateIdAsync(candidateId);

        return _mapper.Map<List<TestInstanceDto>>(tests);
    }

    public async Task<TestInstanceDto> StartTestAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdWithSnapshotsAsync(testId);

        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        // IDOR Protection
        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        // Validate status
        if (test.Status != TestStatus.NotStarted)
        {
            throw new InvalidOperationException($"Test {testId} cannot be started (status: {test.Status})");
        }

        // Start test
        test.Status = TestStatus.InProgress;
        test.StartedAt = DateTimeOffset.UtcNow;

        await _testInstanceRepo.UpdateAsync(test);
        await _testInstanceRepo.SaveAsync();

        _logger.LogInformation("Started test {TestId} for candidate {CandidateId}", testId, candidateId);

        return _mapper.Map<TestInstanceDto>(test);
    }

    public async Task<TestSubmissionResultDto> SubmitTestAsync(SubmitTestRequest request)
    {
        var test = await _testInstanceRepo.GetByIdFullAsync(request.TestId);

        if (test == null)
        {
            throw new NotFoundException("TestInstance", request.TestId);
        }

        // IDOR Protection
        if (test.CandidateId != request.CandidateId)
        {
            throw new UnauthorizedAccessException($"Test {request.TestId} does not belong to candidate {request.CandidateId}");
        }

        // Validate status
        if (test.Status != TestStatus.InProgress)
        {
            throw new InvalidOperationException($"Test {request.TestId} cannot be submitted (status: {test.Status})");
        }

        // Save answers as TestQuestionResponse entities
        var responses = new List<TestQuestionResponse>();
        foreach (var answer in request.Answers)
        {
            var snapshot = test.QuestionSnapshots.FirstOrDefault(q => q.Id == answer.QuestionSnapshotId);
            if (snapshot == null)
            {
                _logger.LogWarning("Question snapshot {SnapshotId} not found in test {TestId}",
                    answer.QuestionSnapshotId, request.TestId);
                continue;
            }

            // Check if response already exists
            var existingResponse = await _questionResponseRepo.GetBySnapshotIdAsync(answer.QuestionSnapshotId);
            if (existingResponse != null && existingResponse.CandidateId == test.CandidateId)
            {
                _logger.LogInformation("Updating existing response for question {QuestionSnapshotId}", answer.QuestionSnapshotId);

                // Update existing response
                existingResponse.SelectedAnswersJson = JsonSerializer.Serialize(answer.SelectedAnswers);
                existingResponse.ResponseTimeMs = answer.ResponseTimeMs;
                existingResponse.AnsweredAt = DateTimeOffset.UtcNow;

                await _questionResponseRepo.UpdateAsync(existingResponse);
                responses.Add(existingResponse);
            }
            else
            {
                // Create new response
                var response = new TestQuestionResponse
                {
                    Id = Guid.NewGuid(),
                    TestInstanceId = test.Id,
                    CandidateId = test.CandidateId,
                    QuestionSnapshotId = snapshot.Id,
                    SelectedAnswersJson = JsonSerializer.Serialize(answer.SelectedAnswers),
                    ResponseTimeMs = answer.ResponseTimeMs,
                    AnsweredAt = DateTimeOffset.UtcNow
                };

                await _questionResponseRepo.AddAsync(response);
                responses.Add(response);
            }
        }

        await _questionResponseRepo.SaveAsync();

        // Auto-grade for objective questions
        await AutoGradeTestAsync(test, responses);

        // Update test status
        test.Status = TestStatus.Submitted;
        test.CompletedAt = DateTimeOffset.UtcNow;
        test.DurationSeconds = test.StartedAt.HasValue
            ? (int)(test.CompletedAt.Value - test.StartedAt.Value).TotalSeconds
            : null;

        await _testInstanceRepo.UpdateAsync(test);
        await _testInstanceRepo.SaveAsync();

        _logger.LogInformation("Submitted test {TestId} for candidate {CandidateId} with score {Score}",
            request.TestId, request.CandidateId, test.Score);

        // Return result
        return new TestSubmissionResultDto
        {
            TestId = test.Id,
            Status = test.Status,
            Score = test.Score,
            RawScore = test.RawScore,
            MaxPossibleScore = test.MaxPossibleScore,
            CorrectAnswers = responses.Count(r => r.IsCorrect == true),
            TotalQuestions = test.QuestionSnapshots.Count,
            DurationSeconds = test.DurationSeconds
        };
    }

    public async Task<List<QuestionSnapshotDto>> GetTestQuestionsAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdWithSnapshotsAsync(testId);

        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        // IDOR Protection
        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        // Map to DTOs (AutoMapper will exclude CorrectAnswerSnapshot)
        return _mapper.Map<List<QuestionSnapshotDto>>(test.QuestionSnapshots.OrderBy(q => q.QuestionOrder));
    }

    public async Task<TestStatusDto> GetTestStatusAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdWithResponsesAsync(testId);

        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        // IDOR Protection
        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        var totalQuestions = test.QuestionSnapshots.Count;
        var questionsAnswered = test.QuestionResponses.Select(r => r.QuestionSnapshotId).Distinct().Count();
        var videosUploaded = test.VideoResponses.Count;

        // Determine required videos for video-based tests
        var videosRequired = test.TestType switch
        {
            TestType.Portuguese => totalQuestions + 1, // 3 questions + 1 reading = 4 videos
            TestType.Math => totalQuestions,           // 2 video questions = 2 videos
            TestType.Interview => totalQuestions,      // 5 video interview questions = 5 videos
            _ => 0                                     // Non-video tests don't require videos
        };

        // Calculate remaining time
        int? remainingSeconds = null;
        if (test.Status == TestStatus.InProgress && test.StartedAt.HasValue)
        {
            remainingSeconds = await GetRemainingTimeAsync(testId);
        }

        return new TestStatusDto
        {
            TestId = test.Id,
            Status = test.Status,
            TotalQuestions = totalQuestions,
            QuestionsAnswered = questionsAnswered,
            VideosUploaded = videosUploaded,
            VideosRequired = videosRequired,
            CanStart = test.Status == TestStatus.NotStarted,
            CanSubmit = test.Status == TestStatus.InProgress &&
                        (test.TestType == TestType.Portuguese || test.TestType == TestType.Math || test.TestType == TestType.Interview
                            ? videosUploaded >= videosRequired
                            : questionsAnswered >= totalQuestions),
            StartedAt = test.StartedAt,
            RemainingTimeSeconds = remainingSeconds
        };
    }

    public async Task<bool> CanStartTestAsync(Guid candidateId, TestType testType)
    {
        // Check for active tests (in progress)
        var activeTest = await _testInstanceRepo.GetActiveByCandidateAndTypeAsync(candidateId, testType);
        if (activeTest != null)
        {
            return false;
        }

        // Also check for completed tests - candidates can only take each test type once
        var allTestsOfType = await _testInstanceRepo.GetByCandidateAndTypeAsync(candidateId, testType);
        var hasCompletedTest = allTestsOfType.Any(t => t.Status == TestStatus.Submitted);
        
        return !hasCompletedTest;
    }

    public async Task<int?> GetRemainingTimeAsync(Guid testId)
    {
        var test = await _testInstanceRepo.GetByIdAsync(testId);

        if (test == null || !test.StartedAt.HasValue || test.Status != TestStatus.InProgress)
        {
            return null;
        }

        var timeLimit = TimeLimitsSeconds.GetValueOrDefault(test.TestType);
        if (!timeLimit.HasValue)
        {
            return null; // No time limit
        }

        var elapsed = (DateTimeOffset.UtcNow - test.StartedAt.Value).TotalSeconds;
        var remaining = timeLimit.Value - (int)elapsed;

        return Math.Max(0, remaining);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Initialize test-type-specific data (e.g., Portuguese reading text)
    /// </summary>
    private async Task InitializeTestTypeSpecificsAsync(TestInstance test, string difficultyLevel)
    {
        if (test.TestType == TestType.Portuguese)
        {
            // Select a random Portuguese reading text based on difficulty
            var readingText = await _portugueseReadingTextRepo.GetRandomTextAsync(difficultyLevel);

            if (readingText != null)
            {
                test.PortugueseReadingTextId = readingText.Id;
                test.PortugueseReadingTextVersion = readingText.Version;

                _logger.LogInformation(
                    "Assigned Portuguese reading text {ReadingTextId} ('{Title}') to test {TestId}",
                    readingText.Id, readingText.Title, test.Id);
            }
            else
            {
                _logger.LogWarning(
                    "No Portuguese reading text found for difficulty '{Difficulty}'. Test {TestId} will not have a reading text assigned.",
                    difficultyLevel, test.Id);
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Snapshot questions from template bank to test instance
    /// For video-based tests (Portuguese/Math), creates generic video question prompts
    /// ⚠️ CRITICAL: Uses .IgnoreQueryFilters() to access QuestionAnswer for template-based tests
    /// </summary>
    private async Task SnapshotQuestionsForTestAsync(TestInstance test, string difficultyLevel)
    {
        var questionCount = QuestionCounts.GetValueOrDefault(test.TestType, 10);

        // Handle video-based tests differently (Portuguese, Math, Interview)
        // These load questions from TestQuestionGroup instead of random selection
        if (test.TestType == TestType.Portuguese || test.TestType == TestType.Math || test.TestType == TestType.Interview)
        {
            await CreateVideoQuestionSlotsAsync(test, questionCount);
            return;
        }

        // Traditional template-based tests (Psychology, VisualRetention)
        List<QuestionTemplate> templates;

        // Psychology tests use ALL questions in order (not random selection)
        // IMPORTANT: Ignore difficulty level - deliver ALL questions regardless of difficulty
        if (test.TestType == TestType.Psychology)
        {
            templates = await _questionTemplateRepo.GetAllQuestionsOrderedAsync(
                test.TestType,
                null); // Pass null to ignore difficulty filter

            if (templates.Count == 0)
            {
                throw new InvalidOperationException(
                    $"No Psychology questions available in database. Please ensure questions are seeded.");
            }

            _logger.LogInformation(
                "Selected ALL {Count} Psychology questions in original order for test {TestId} (ignoring difficulty filter)",
                templates.Count, test.Id);
        }
        else
        {
            // Other test types use random selection
            templates = await _questionTemplateRepo.GetRandomQuestionsAsync(
                test.TestType,
                questionCount,
                difficultyLevel);

            if (templates.Count < questionCount)
            {
                throw new InvalidOperationException(
                    $"Not enough questions available for {test.TestType} (requested: {questionCount}, available: {templates.Count})");
            }
        }

        // Create snapshots
        var snapshots = new List<TestQuestionSnapshot>();
        for (int i = 0; i < templates.Count; i++)
        {
            var template = templates[i];

            // Log warning if Answer is missing
            if (template.Answer == null)
            {
                _logger.LogWarning("QuestionTemplate {TemplateId} has no Answer entity - correct answers will not be available for grading", template.Id);
            }

            var snapshot = new TestQuestionSnapshot
            {
                Id = Guid.NewGuid(),
                TestInstanceId = test.Id,
                QuestionTemplateId = template.Id,
                QuestionText = template.QuestionText,
                OptionsJson = template.OptionsJson,
                AllowMultipleAnswers = template.AllowMultipleAnswers,
                MaxAnswersAllowed = template.MaxAnswersAllowed,
                QuestionOrder = i + 1,
                PointValue = template.PointValue,
                EstimatedTimeSeconds = template.EstimatedTimeSeconds,
                // ⚠️ SECURITY: Snapshot correct answer (stored securely, never sent to client)
                CorrectAnswerSnapshot = template.Answer?.CorrectAnswer,
                ExpectedAnswerGuideSnapshot = template.Answer?.ExpectedAnswerGuideJson
            };

            snapshots.Add(snapshot);
        }

        await _snapshotRepo.AddRangeAsync(snapshots);
        await _snapshotRepo.SaveAsync();

        // Populate the navigation property on the test instance
        // This is important for InMemory databases where re-fetching may not work correctly
        test.QuestionSnapshots = snapshots;

        _logger.LogInformation("Snapshotted {Count} questions for test {TestId}", snapshots.Count, test.Id);
    }

    /// <summary>
    /// Create video question slots for Portuguese, Math, and Interview tests
    /// Loads questions from database TestQuestionGroup
    /// </summary>
    private async Task CreateVideoQuestionSlotsAsync(TestInstance test, int questionCount)
    {
        // Get the active question group for this test type
        var questionGroup = await _questionGroupRepo.GetActiveGroupByTestTypeAsync(test.TestType);

        if (questionGroup == null)
        {
            _logger.LogWarning(
                "No active question group found for {TestType}. Creating generic video question slots.",
                test.TestType);

            // Fallback to generic questions if no group is configured
            await CreateGenericVideoQuestionSlotsAsync(test, questionCount);
            return;
        }

        // Load group with questions ordered by GroupOrder
        var groupWithQuestions = await _questionGroupRepo.GetByIdWithQuestionsAsync(questionGroup.Id);
        if (groupWithQuestions == null || !groupWithQuestions.Questions.Any())
        {
            _logger.LogWarning(
                "Question group {GroupId} for {TestType} has no questions. Creating generic video question slots.",
                questionGroup.Id, test.TestType);

            await CreateGenericVideoQuestionSlotsAsync(test, questionCount);
            return;
        }

        // Create snapshots from the grouped questions
        var snapshots = new List<TestQuestionSnapshot>();
        var questionsOrdered = groupWithQuestions.Questions
            .OrderBy(q => q.GroupOrder ?? int.MaxValue)
            .ToList();

        for (int i = 0; i < questionsOrdered.Count && i < questionCount; i++)
        {
            var template = questionsOrdered[i];

            var snapshot = new TestQuestionSnapshot
            {
                Id = Guid.NewGuid(),
                TestInstanceId = test.Id,
                QuestionTemplateId = template.Id,
                QuestionTemplateVersion = template.Version,
                QuestionText = template.QuestionText,
                OptionsJson = template.OptionsJson, // May be null for open-ended questions
                AllowMultipleAnswers = template.AllowMultipleAnswers,
                MaxAnswersAllowed = template.MaxAnswersAllowed,
                QuestionOrder = i + 1,
                PointValue = template.PointValue,
                EstimatedTimeSeconds = template.EstimatedTimeSeconds,
                // Video responses are manually graded, so we don't snapshot correct answers
                CorrectAnswerSnapshot = null,
                ExpectedAnswerGuideSnapshot = template.Answer?.ExpectedAnswerGuideJson
            };

            snapshots.Add(snapshot);
        }

        // If we don't have enough questions from the group, fill with generic ones
        if (snapshots.Count < questionCount)
        {
            _logger.LogWarning(
                "Question group {GroupId} only has {QuestionCount} questions, but {RequiredCount} are needed. Filling with generic questions.",
                questionGroup.Id, snapshots.Count, questionCount);

            for (int i = snapshots.Count; i < questionCount; i++)
            {
                var questionText = test.TestType == TestType.Portuguese
                    ? $"Question {i + 1}: Please answer this question about the reading text by recording a video."
                    : test.TestType == TestType.Math
                        ? $"Math Question {i + 1}: Please solve this problem and explain your solution in a video."
                        : $"Question {i + 1}: Please answer this question by recording a video.";

                var snapshot = new TestQuestionSnapshot
                {
                    Id = Guid.NewGuid(),
                    TestInstanceId = test.Id,
                    QuestionTemplateId = null,
                    QuestionText = questionText,
                    OptionsJson = null,
                    AllowMultipleAnswers = false,
                    MaxAnswersAllowed = null,
                    QuestionOrder = i + 1,
                    PointValue = 1.0m,
                    EstimatedTimeSeconds = null,
                    CorrectAnswerSnapshot = null,
                    ExpectedAnswerGuideSnapshot = null
                };

                snapshots.Add(snapshot);
            }
        }

        await _snapshotRepo.AddRangeAsync(snapshots);
        await _snapshotRepo.SaveAsync();

        // Populate the navigation property on the test instance
        test.QuestionSnapshots = snapshots;

        _logger.LogInformation(
            "Created {QuestionCount} video question snapshots for {TestType} test {TestId} from question group {GroupId} ('{GroupName}')",
            snapshots.Count, test.TestType, test.Id, questionGroup.Id, questionGroup.GroupName);
    }

    /// <summary>
    /// Fallback method to create generic video question slots when no question group is configured
    /// </summary>
    private async Task CreateGenericVideoQuestionSlotsAsync(TestInstance test, int questionCount)
    {
        var snapshots = new List<TestQuestionSnapshot>();

        for (int i = 1; i <= questionCount; i++)
        {
            var questionText = test.TestType == TestType.Portuguese
                ? $"Question {i}: Please answer this question about the reading text by recording a video."
                : test.TestType == TestType.Math
                    ? $"Math Question {i}: Please solve this problem and explain your solution in a video."
                    : $"Question {i}: Please answer this question by recording a video.";

            var snapshot = new TestQuestionSnapshot
            {
                Id = Guid.NewGuid(),
                TestInstanceId = test.Id,
                QuestionTemplateId = null,
                QuestionText = questionText,
                OptionsJson = null,
                AllowMultipleAnswers = false,
                MaxAnswersAllowed = null,
                QuestionOrder = i,
                PointValue = 1.0m,
                EstimatedTimeSeconds = null,
                CorrectAnswerSnapshot = null,
                ExpectedAnswerGuideSnapshot = null
            };

            snapshots.Add(snapshot);
        }

        await _snapshotRepo.AddRangeAsync(snapshots);
        await _snapshotRepo.SaveAsync();

        test.QuestionSnapshots = snapshots;

        _logger.LogInformation(
            "Created {QuestionCount} generic video question slots for {TestType} test {TestId}",
            snapshots.Count, test.TestType, test.Id);
    }

    /// <summary>
    /// Auto-grade objective questions by comparing answers to snapshots
    /// ⚠️ SECURITY: Only done server-side, never exposes correct answers to client
    /// </summary>
    private async Task AutoGradeTestAsync(TestInstance test, List<TestQuestionResponse> responses)
    {
        decimal totalPoints = 0;
        decimal earnedPoints = 0;

        foreach (var response in responses)
        {
            var snapshot = test.QuestionSnapshots.FirstOrDefault(q => q.Id == response.QuestionSnapshotId);
            if (snapshot == null)
            {
                continue;
            }

            totalPoints += snapshot.PointValue;

            // Compare selected answers to correct answer snapshot
            if (string.IsNullOrEmpty(snapshot.CorrectAnswerSnapshot))
            {
                // No correct answer defined (e.g., subjective question)
                continue;
            }

            var selectedAnswers = JsonSerializer.Deserialize<List<string>>(response.SelectedAnswersJson) ?? new List<string>();
            var correctAnswers = JsonSerializer.Deserialize<List<string>>(snapshot.CorrectAnswerSnapshot) ?? new List<string>();

            // Compare answers (order-independent)
            var isCorrect = selectedAnswers.OrderBy(a => a).SequenceEqual(correctAnswers.OrderBy(a => a));

            response.IsCorrect = isCorrect;
            response.PointsEarned = isCorrect ? snapshot.PointValue : 0;

            if (isCorrect)
            {
                earnedPoints += snapshot.PointValue;
            }

            await _questionResponseRepo.UpdateAsync(response);
        }

        await _questionResponseRepo.SaveAsync();

        // Update test scores
        test.RawScore = earnedPoints;
        test.MaxPossibleScore = totalPoints;
        test.Score = totalPoints > 0 ? Math.Round((earnedPoints / totalPoints) * 100, 2) : 0;

        _logger.LogInformation("Auto-graded test {TestId}: {EarnedPoints}/{TotalPoints} points ({Score}%)",
            test.Id, earnedPoints, totalPoints, test.Score);
    }

    #endregion
}
