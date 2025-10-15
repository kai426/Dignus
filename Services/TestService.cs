using AutoMapper;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Candidate.Back.Configuration;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;
using Microsoft.Extensions.Options;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service implementation for test management operations
    /// </summary>
    public class TestService : ITestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<TestService> _logger;
        private readonly TestSettings _testSettings;

        public TestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TestService> logger,
            IOptions<TestSettings> testSettings)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _testSettings = testSettings.Value;
        }

        public async Task<BaseTestDto> CreateTestAsync(CreateTestDto createTestDto)
        {
            try
            {
                // Validate candidate exists
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(createTestDto.CandidateId);
                if (candidate == null)
                {
                    throw new InvalidOperationException($"Candidate with ID {createTestDto.CandidateId} not found.");
                }

                // Check if candidate can start this test type
                if (!await CanStartTestAsync(createTestDto.CandidateId, createTestDto.TestType))
                {
                    throw new InvalidOperationException($"Candidate cannot start test of type {createTestDto.TestType}.");
                }

                BaseTest test = createTestDto.TestType.ToLower() switch
                {
                    "math" => new MathTest(),
                    "portuguese" => new PortugueseTest(),
                    "psychology" => new PsychologyTest(),
                    _ => throw new ArgumentException($"Invalid test type: {createTestDto.TestType}")
                };

                test.Id = Guid.NewGuid();
                test.Candidate = candidate;
                test.Status = TestStatus.NotStarted;

                switch (createTestDto.TestType.ToLower())
                {
                    case "math":
                        await _unitOfWork.MathTests.AddAsync((MathTest)test);
                        break;
                    case "portuguese":
                        await _unitOfWork.PortugueseTests.AddAsync((PortugueseTest)test);
                        break;
                    case "psychology":
                        await _unitOfWork.PsychologyTests.AddAsync((PsychologyTest)test);
                        break;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created {TestType} test with ID {TestId} for candidate {CandidateId}", 
                    createTestDto.TestType, test.Id, createTestDto.CandidateId);

                return _mapper.Map<BaseTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test of type {TestType} for candidate {CandidateId}", 
                    createTestDto.TestType, createTestDto.CandidateId);
                throw;
            }
        }

        public async Task<BaseTestDto?> StartTestAsync(Guid testId, Guid candidateId)
        {
            try
            {
                var test = await FindTestByIdAsync(testId);
                if (test == null)
                {
                    return null;
                }

                // Verify test belongs to candidate
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null || test.Candidate.Id != candidateId)
                {
                    return null;
                }

                // Check if test can be started
                if (test.Status != TestStatus.NotStarted)
                {
                    throw new InvalidOperationException($"Test {testId} cannot be started. Current status: {test.Status}");
                }

                // Start the test
                test.Status = TestStatus.InProgress;
                test.StartedAt = DateTimeOffset.UtcNow;

                // Load questions for the test only if not already loaded
                if (!test.Questions.Any())
                {
                    var testType = GetTestType(test);
                    var questionCount = GetQuestionCount(testType);
                    var questions = await GetRandomQuestionsAsync(testType, questionCount);

                    // Add questions to test
                    foreach (var questionDto in questions)
                    {
                        // Get existing question from database instead of creating new ones
                        var existingQuestion = await _unitOfWork.Questions.GetByIdAsync(questionDto.Id);
                        if (existingQuestion != null)
                        {
                            test.Questions.Add(existingQuestion);
                        }
                    }
                }

                UpdateTest(test);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Started test {TestId} for candidate {CandidateId}", testId, candidateId);

                return _mapper.Map<BaseTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting test {TestId} for candidate {CandidateId}", testId, candidateId);
                throw;
            }
        }

        public async Task<BaseTestDto?> SubmitTestAsync(SubmitTestDto submitTestDto, Guid candidateId)
        {
            try
            {
                var test = await FindTestByIdAsync(submitTestDto.TestId);
                if (test == null)
                {
                    return null;
                }

                // Verify test belongs to candidate
                if (test.Candidate.Id != candidateId)
                {
                    return null;
                }

                // Check if test can be submitted
                if (test.Status != TestStatus.InProgress)
                {
                    throw new InvalidOperationException($"Test {submitTestDto.TestId} cannot be submitted. Current status: {test.Status}");
                }

                // Process answers and calculate score
                await ProcessAnswersAsync(test, submitTestDto.Answers);
                
                test.Status = TestStatus.Submitted;
                test.CompletedAt = DateTimeOffset.UtcNow;
                test.Score = await CalculateTestScoreAsync(submitTestDto.TestId);

                UpdateTest(test);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Submitted test {TestId} for candidate {CandidateId} with score {Score}", 
                    submitTestDto.TestId, candidateId, test.Score);

                return _mapper.Map<BaseTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting test {TestId} for candidate {CandidateId}", 
                    submitTestDto.TestId, candidateId);
                throw;
            }
        }

        public async Task<BaseTestDto?> GetTestByIdAsync(Guid testId, Guid candidateId)
        {
            try
            {
                var test = await FindTestByIdAsync(testId);
                if (test == null || test.Candidate.Id != candidateId)
                {
                    return null;
                }

                return _mapper.Map<BaseTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving test {TestId} for candidate {CandidateId}", testId, candidateId);
                throw;
            }
        }

        public async Task<IEnumerable<BaseTestDto>> GetTestsByCandidateIdAsync(Guid candidateId)
        {
            try
            {
                var tests = new List<BaseTest>();

                // Get all test types for the candidate
                var mathTests = await _unitOfWork.MathTests.GetByCandidateIdAsync(candidateId);
                var portugueseTests = await _unitOfWork.PortugueseTests.GetByCandidateIdAsync(candidateId);
                var psychologyTests = await _unitOfWork.PsychologyTests.GetByCandidateIdAsync(candidateId);

                tests.AddRange(mathTests);
                tests.AddRange(portugueseTests);
                tests.AddRange(psychologyTests);

                return _mapper.Map<List<BaseTestDto>>(tests);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tests for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<IEnumerable<QuestionDto>> GetRandomQuestionsAsync(string testType, int questionCount = 10)
        {
            try
            {
                var allQuestions = await _unitOfWork.Questions.GetAllAsync();
                var filteredQuestions = allQuestions
                    .Where(q => q.Type != null && q.Type.Equals(testType, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => Guid.NewGuid()) // Random ordering
                    .Take(questionCount)
                    .ToList();

                return _mapper.Map<List<QuestionDto>>(filteredQuestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving random questions for test type {TestType}", testType);
                throw;
            }
        }

        public async Task<decimal> CalculateTestScoreAsync(Guid testId)
        {
            try
            {
                // This is a simplified scoring algorithm
                // In a real implementation, you would have more sophisticated scoring logic
                
                var test = await FindTestByIdAsync(testId);
                if (test == null || !test.Questions.Any())
                {
                    return 0;
                }

                var totalQuestions = test.Questions.Count;
                var correctAnswers = 0;

                // Placeholder scoring - implement actual logic based on correct answers
                correctAnswers = totalQuestions / 2; // Simplified for demo

                var score = (decimal)correctAnswers / totalQuestions * 100;
                return Math.Round(score, 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating score for test {TestId}", testId);
                throw;
            }
        }

        public async Task<bool> CanStartTestAsync(Guid candidateId, string testType)
        {
            try
            {
                // Check if candidate already has a test of this type in progress
                var existingTests = await GetTestsByCandidateIdAsync(candidateId);
                var testOfType = existingTests.FirstOrDefault(t => GetTestType(t) == testType);

                if (testOfType != null)
                {
                    return testOfType.Status == TestStatus.NotStarted;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start test type {TestType}", 
                    candidateId, testType);
                throw;
            }
        }

        public async Task<Dictionary<string, bool>> GetTestStatusAsync(Guid candidateId)
        {
            try
            {
                var tests = await GetTestsByCandidateIdAsync(candidateId);
                var status = new Dictionary<string, bool>
                {
                    { "Math", false },
                    { "Portuguese", false },
                    { "Psychology", false }
                };

                foreach (var test in tests)
                {
                    var testType = GetTestType(test);
                    status[testType] = test.Status == TestStatus.Submitted;
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting test status for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        // Private helper methods

        private async Task<BaseTest?> FindTestByIdAsync(Guid testId)
        {
            // Try to find in each test type repository using GetWithQuestionsAsync to include Candidate
            var mathTest = await _unitOfWork.MathTests.GetWithQuestionsAsync(testId);
            if (mathTest != null) return mathTest;

            var portugueseTest = await _unitOfWork.PortugueseTests.GetWithQuestionsAsync(testId);
            if (portugueseTest != null) return portugueseTest;

            var psychologyTest = await _unitOfWork.PsychologyTests.GetWithQuestionsAsync(testId);
            return psychologyTest;
        }

        private void UpdateTest(BaseTest test)
        {
            switch (test)
            {
                case MathTest mathTest:
                    _unitOfWork.MathTests.Update(mathTest);
                    break;
                case PortugueseTest portugueseTest:
                    _unitOfWork.PortugueseTests.Update(portugueseTest);
                    break;
                case PsychologyTest psychologyTest:
                    _unitOfWork.PsychologyTests.Update(psychologyTest);
                    break;
            }
        }

        private string GetTestType(BaseTest test)
        {
            return test switch
            {
                MathTest => "Math",
                PortugueseTest => "Portuguese",
                PsychologyTest => "Psychology",
                _ => "Unknown"
            };
        }

        private string GetTestType(BaseTestDto testDto)
        {
            return testDto switch
            {
                MathTestDto => "Math",
                PortugueseTestDto => "Portuguese", 
                PsychologyTestDto => "Psychology",
                _ => "Unknown"
            };
        }

        private Task ProcessAnswersAsync(BaseTest test, List<QuestionResponseDto> answers)
        {
            // Process each answer and create QuestionResponse entities
            foreach (var answerDto in answers)
            {
                var question = test.Questions.FirstOrDefault(q => q.Id == answerDto.QuestionId);
                if (question != null)
                {
                    var response = new QuestionResponse
                    {
                        Id = Guid.NewGuid(),
                        Question = question,
                        Candidate = test.Candidate,
                        SelectedAnswers = answerDto.SelectedAnswers,
                        AnsweredAt = DateTimeOffset.UtcNow
                    };

                    // This would need to be handled by the repository pattern
                    // For now, we'll assume it's handled elsewhere
                }
            }
            
            return Task.CompletedTask;
        }

        private decimal CalculateQuestionScore(Question question, QuestionResponseDto answer)
        {
            // Simplified scoring logic - compare selected answers with correct answer
            if (!string.IsNullOrEmpty(question.CorrectAnswer) && 
                answer.SelectedAnswers.Equals(question.CorrectAnswer, StringComparison.OrdinalIgnoreCase))
            {
                return 1.0m;
            }
            return 0.0m;
        }

        public async Task<IEnumerable<BaseTestDto>> GetOrCreateTestsForCandidateAsync(Guid candidateId)
        {
            try
            {
                // Get existing tests
                var existingTests = await GetTestsByCandidateIdAsync(candidateId);
                var existingTestTypes = existingTests.Select(t => GetTestType(t)).ToHashSet();

                var allTestTypes = new[] { "Math", "Portuguese", "Psychology" };
                var testsToCreate = allTestTypes.Where(type => !existingTestTypes.Contains(type)).ToList();

                // Create missing tests
                foreach (var testType in testsToCreate)
                {
                    var createTestDto = new CreateTestDto
                    {
                        CandidateId = candidateId,
                        TestType = testType
                    };

                    try
                    {
                        await CreateTestAsync(createTestDto);
                        _logger.LogInformation("Auto-created {TestType} test for candidate {CandidateId}", testType, candidateId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to auto-create {TestType} test for candidate {CandidateId}", testType, candidateId);
                    }
                }

                // Return all tests (existing + newly created)
                return await GetTestsByCandidateIdAsync(candidateId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating tests for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<BaseTestDto?> GetOrCreateTestAsync(Guid candidateId, string testType)
        {
            try
            {
                _logger.LogInformation("Getting or creating {TestType} test for candidate {CandidateId}", testType, candidateId);

                // Check if candidate has existing test of this type
                var existingTest = await FindExistingTestByTypeAsync(candidateId, testType);
                if (existingTest != null)
                {
                    _logger.LogInformation("Found existing {TestType} test {TestId} for candidate {CandidateId}", testType, existingTest.Id, candidateId);
                    // Return existing test with questions loaded
                    return await LoadTestWithQuestionsAsync(existingTest);
                }

                // Create new test with questions
                var newTest = await CreateTestWithQuestionsAsync(candidateId, testType);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new {TestType} test {TestId} for candidate {CandidateId}", testType, newTest.Id, candidateId);
                return await LoadTestWithQuestionsAsync(newTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating {TestType} test for candidate {CandidateId}", testType, candidateId);
                throw;
            }
        }

        private async Task<BaseTest?> FindExistingTestByTypeAsync(Guid candidateId, string testType)
        {
            return testType.ToLower() switch
            {
                "portuguese" => (await _unitOfWork.PortugueseTests.GetByCandidateIdAsync(candidateId))
                    .FirstOrDefault(),
                "math" => (await _unitOfWork.MathTests.GetByCandidateIdAsync(candidateId))
                    .FirstOrDefault(),
                "psychology" => (await _unitOfWork.PsychologyTests.GetByCandidateIdAsync(candidateId))
                    .FirstOrDefault(),
                "visualretention" => (await _unitOfWork.VisualRetentionTests.GetByCandidateIdAsync(candidateId))
                    .FirstOrDefault(),
                _ => null
            };
        }

        private async Task<BaseTest> CreateTestWithQuestionsAsync(Guid candidateId, string testType)
        {
            var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
            if (candidate == null)
                throw new InvalidOperationException($"Candidate {candidateId} not found");

            BaseTest test = testType.ToLower() switch
            {
                "portuguese" => new PortugueseTest(),
                "math" => new MathTest(),
                "psychology" => new PsychologyTest(),
                "visualretention" => new VisualRetentionTest(),
                _ => throw new ArgumentException($"Invalid test type: {testType}")
            };

            test.Id = Guid.NewGuid();
            test.Candidate = candidate;
            test.Status = TestStatus.NotStarted;
            // Set StartedAt when test is created (ready to be started)
            // test.StartedAt = DateTimeOffset.UtcNow; // Don't set this until test is actually started

            // Add questions based on test type
            await AddQuestionsToTestAsync(test, testType);

            // Save to appropriate repository
            await SaveTestToRepositoryAsync(test, testType);

            return test;
        }

        private async Task AddQuestionsToTestAsync(BaseTest test, string testType)
        {
            if (IsFixedQuestionTest(testType))
            {
                // For Psychology and Visual Retention - get ALL fixed questions
                var allQuestions = await _unitOfWork.Questions.GetByTypeAsync(testType);
                foreach (var question in allQuestions)
                {
                    test.Questions.Add(question);
                }
            }
            else
            {
                // For Portuguese and Math - get random question IDs
                var questionCount = GetQuestionCount(testType);
                var randomQuestionDtos = await GetRandomQuestionsAsync(testType, questionCount);

                // Get the actual Question entities from the database by their IDs
                foreach (var questionDto in randomQuestionDtos)
                {
                    var question = await _unitOfWork.Questions.GetByIdAsync(questionDto.Id);
                    if (question != null)
                    {
                        test.Questions.Add(question);
                    }
                }
            }
        }

        private bool IsFixedQuestionTest(string testType)
        {
            return testType.ToLower() is "psychology" or "visualretention";
        }

        private async Task SaveTestToRepositoryAsync(BaseTest test, string testType)
        {
            switch (testType.ToLower())
            {
                case "portuguese":
                    await _unitOfWork.PortugueseTests.AddAsync((PortugueseTest)test);
                    break;
                case "math":
                    await _unitOfWork.MathTests.AddAsync((MathTest)test);
                    break;
                case "psychology":
                    await _unitOfWork.PsychologyTests.AddAsync((PsychologyTest)test);
                    break;
                case "visualretention":
                    await _unitOfWork.VisualRetentionTests.AddAsync((VisualRetentionTest)test);
                    break;
                default:
                    throw new ArgumentException($"Invalid test type: {testType}");
            }
        }

        private async Task<BaseTestDto> LoadTestWithQuestionsAsync(BaseTest test)
        {
            // Load questions if not already loaded
            if (!test.Questions.Any())
            {
                var testType = GetTestType(test);
                var questions = await _unitOfWork.Questions.GetByTypeAsync(testType);

                // Add questions to the test entity
                foreach (var question in questions)
                {
                    test.Questions.Add(question);
                }
            }

            // Map to appropriate DTO type based on test type
            return test switch
            {
                PortugueseTest portugueseTest => _mapper.Map<PortugueseTestDto>(portugueseTest),
                MathTest mathTest => _mapper.Map<MathTestDto>(mathTest),
                PsychologyTest psychologyTest => _mapper.Map<PsychologyTestDto>(psychologyTest),
                VisualRetentionTest visualRetentionTest => _mapper.Map<VisualRetentionTestDto>(visualRetentionTest),
                _ => throw new ArgumentException($"Unsupported test type: {test.GetType()}")
            };
        }

        public async Task<bool> IsTestTimedOutAsync(Guid testId)
        {
            try
            {
                var test = await FindTestByIdAsync(testId);
                if (test == null || test.StartedAt == null || test.Status != TestStatus.InProgress)
                {
                    return false;
                }

                var testType = GetTestType(test);
                var timeoutMinutes = GetTestTimeout(testType);
                var timeoutTime = test.StartedAt.Value.AddMinutes(timeoutMinutes);

                return DateTimeOffset.UtcNow > timeoutTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking timeout for test {TestId}", testId);
                throw;
            }
        }

        public async Task<int?> GetRemainingTimeAsync(Guid testId)
        {
            try
            {
                var test = await FindTestByIdAsync(testId);
                if (test == null || test.StartedAt == null || test.Status != TestStatus.InProgress)
                {
                    return null;
                }

                var testType = GetTestType(test);
                var timeoutMinutes = GetTestTimeout(testType);
                var timeoutTime = test.StartedAt.Value.AddMinutes(timeoutMinutes);
                var remaining = timeoutTime - DateTimeOffset.UtcNow;

                return remaining.TotalMinutes > 0 ? (int)Math.Ceiling(remaining.TotalMinutes) : 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting remaining time for test {TestId}", testId);
                throw;
            }
        }

        private int GetQuestionCount(string testType)
        {
            return testType.ToLower() switch
            {
                "math" => _testSettings.QuestionCounts.Math,
                "portuguese" => _testSettings.QuestionCounts.Portuguese,
                "psychology" => _testSettings.QuestionCounts.Psychology,
                "visualretention" => _testSettings.QuestionCounts.VisualRetention,
                _ => 10 // Default fallback
            };
        }

        private int GetTestTimeout(string testType)
        {
            return testType.ToLower() switch
            {
                "math" => _testSettings.TestTimeouts.Math,
                "portuguese" => _testSettings.TestTimeouts.Portuguese,
                "psychology" => _testSettings.TestTimeouts.Psychology,
                "visualretention" => _testSettings.TestTimeouts.VisualRetention,
                _ => 30 // Default fallback in minutes
            };
        }
    }
}