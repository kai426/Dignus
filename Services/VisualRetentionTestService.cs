using AutoMapper;
using Microsoft.Extensions.Options;
using Dignus.Candidate.Back.Configuration;
using Dignus.Candidate.Back.DTOs;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models;
using Dignus.Data.Repositories.Interfaces;

namespace Dignus.Candidate.Back.Services
{
    /// <summary>
    /// Service for Visual Retention Test operations with fixed questions
    /// </summary>
    public class VisualRetentionTestService : IVisualRetentionTestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly TestSettings _testSettings;
        private readonly ILogger<VisualRetentionTestService> _logger;

        public VisualRetentionTestService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IOptions<TestSettings> testSettings,
            ILogger<VisualRetentionTestService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _testSettings = testSettings.Value;
            _logger = logger;
        }

        public async Task<VisualRetentionTestDto?> GetOrCreateVisualRetentionTestAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Getting or creating Visual Retention test for candidate {CandidateId}", candidateId);

                // Check if candidate exists
                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    _logger.LogWarning("Candidate {CandidateId} not found", candidateId);
                    return null;
                }

                // Check for existing visual retention test
                var existingTests = await _unitOfWork.VisualRetentionTests.GetAllAsync();
                var existingTest = existingTests.FirstOrDefault(t => t.Candidate.Id == candidateId);

                if (existingTest != null)
                {
                    _logger.LogInformation("Returning existing Visual Retention test {TestId} for candidate {CandidateId}",
                        existingTest.Id, candidateId);

                    // Load questions if not already loaded
                    await LoadFixedQuestionsAsync(existingTest);
                    return _mapper.Map<VisualRetentionTestDto>(existingTest);
                }

                // Create new visual retention test with all fixed questions
                var newTest = new VisualRetentionTest
                {
                    Id = Guid.NewGuid(),
                    Candidate = candidate,
                    Status = TestStatus.NotStarted,
                    Questions = new List<Question>()
                };

                // Load all fixed visual retention questions
                await LoadFixedQuestionsAsync(newTest);

                await _unitOfWork.VisualRetentionTests.AddAsync(newTest);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created new Visual Retention test {TestId} for candidate {CandidateId}",
                    newTest.Id, candidateId);

                return _mapper.Map<VisualRetentionTestDto>(newTest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating Visual Retention test for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<VisualRetentionTestDto?> GetVisualRetentionTestByIdAsync(Guid testId, Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Getting Visual Retention test {TestId} for candidate {CandidateId}", testId, candidateId);

                var test = await _unitOfWork.VisualRetentionTests.GetByIdAsync(testId);
                if (test == null || test.Candidate?.Id != candidateId)
                {
                    _logger.LogWarning("Visual Retention test {TestId} not found or doesn't belong to candidate {CandidateId}",
                        testId, candidateId);
                    return null;
                }

                await LoadFixedQuestionsAsync(test);
                return _mapper.Map<VisualRetentionTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Visual Retention test {TestId} for candidate {CandidateId}", testId, candidateId);
                throw;
            }
        }

        public async Task<VisualRetentionTestDto?> StartVisualRetentionTestAsync(Guid testId, Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Starting Visual Retention test {TestId} for candidate {CandidateId}", testId, candidateId);

                var test = await _unitOfWork.VisualRetentionTests.GetByIdAsync(testId);
                if (test == null || test.Candidate?.Id != candidateId)
                {
                    _logger.LogWarning("Visual Retention test {TestId} not found or doesn't belong to candidate {CandidateId}",
                        testId, candidateId);
                    return null;
                }

                if (test.Status != TestStatus.NotStarted)
                {
                    throw new InvalidOperationException($"Visual Retention test is already {test.Status}");
                }

                test.Status = TestStatus.InProgress;
                test.StartedAt = DateTimeOffset.UtcNow;

                _unitOfWork.VisualRetentionTests.Update(test);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Started Visual Retention test {TestId} for candidate {CandidateId}", testId, candidateId);

                await LoadFixedQuestionsAsync(test);
                return _mapper.Map<VisualRetentionTestDto>(test);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Visual Retention test {TestId} for candidate {CandidateId}", testId, candidateId);
                throw;
            }
        }

        public async Task<bool> SubmitTestResponsesAsync(VisualRetentionSubmissionDto submission)
        {
            try
            {
                _logger.LogInformation("Submitting Visual Retention test responses for candidate {CandidateId}, test {TestId}",
                    submission.CandidateId, submission.TestId);

                // Validate submission
                if (submission.FixedResponses == null || !submission.FixedResponses.Any())
                {
                    _logger.LogWarning("No responses provided in Visual Retention test submission");
                    return false;
                }

                var test = await _unitOfWork.VisualRetentionTests.GetByIdAsync(submission.TestId);
                if (test == null || test.Candidate?.Id != submission.CandidateId)
                {
                    _logger.LogWarning("Visual Retention test {TestId} not found or doesn't belong to candidate {CandidateId}",
                        submission.TestId, submission.CandidateId);
                    return false;
                }

                // Validate test can be submitted
                if (test.Status != TestStatus.InProgress)
                {
                    _logger.LogWarning("Visual Retention test {TestId} is not in progress (status: {Status})",
                        submission.TestId, test.Status);
                    return false;
                }

                // Process each fixed response (A, B, C, D, E format)
                var processedResponses = new Dictionary<string, string>();
                foreach (var response in submission.FixedResponses)
                {
                    var questionNumber = response.Key;
                    var selectedOption = response.Value.ToUpper();

                    // Validate option is A, B, C, D, or E
                    if (!new[] { "A", "B", "C", "D", "E" }.Contains(selectedOption))
                    {
                        _logger.LogWarning("Invalid response option '{Option}' for question {QuestionNumber}",
                            selectedOption, questionNumber);
                        continue;
                    }

                    processedResponses[questionNumber] = selectedOption;
                }

                // Calculate basic scores if provided
                var accuracyScore = submission.AccuracyScore ?? 0;
                var speedScore = submission.SpeedScore ?? 0;

                // Update test with responses and completion status
                test.Status = TestStatus.Submitted;
                test.CompletedAt = submission.SubmittedAt;
                test.Score = (accuracyScore + speedScore) / 2; // Simple average for now

                // In a real implementation, you would store the individual responses
                // For now, we'll just mark the test as submitted
                _unitOfWork.VisualRetentionTests.Update(test);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully processed {ResponseCount} Visual Retention test responses for candidate {CandidateId}",
                    processedResponses.Count, submission.CandidateId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting Visual Retention test responses for candidate {CandidateId}",
                    submission.CandidateId);
                return false;
            }
        }

        public async Task<object> GetTestStatusAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Getting Visual Retention test status for candidate {CandidateId}", candidateId);

                var tests = await _unitOfWork.VisualRetentionTests.GetAllAsync();
                var test = tests.FirstOrDefault(t => t.Candidate.Id == candidateId);

                if (test == null)
                {
                    return new { hasTest = false, status = "NotStarted", canStart = true };
                }

                return new
                {
                    hasTest = true,
                    testId = test.Id,
                    status = test.Status.ToString(),
                    startedAt = test.StartedAt,
                    completedAt = test.CompletedAt,
                    score = test.Score,
                    canStart = test.Status == TestStatus.NotStarted,
                    canContinue = test.Status == TestStatus.InProgress
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Visual Retention test status for candidate {CandidateId}", candidateId);
                throw;
            }
        }

        public async Task<bool> CanStartTestAsync(Guid candidateId)
        {
            try
            {
                _logger.LogInformation("Checking if candidate {CandidateId} can start Visual Retention test", candidateId);

                var candidate = await _unitOfWork.Candidates.GetByIdAsync(candidateId);
                if (candidate == null)
                {
                    return false;
                }

                var tests = await _unitOfWork.VisualRetentionTests.GetAllAsync();
                var existingTest = tests.FirstOrDefault(t => t.Candidate.Id == candidateId);

                // Can start if no test exists or if test is not started
                return existingTest == null || existingTest.Status == TestStatus.NotStarted;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if candidate {CandidateId} can start Visual Retention test", candidateId);
                return false;
            }
        }

        private async Task LoadFixedQuestionsAsync(VisualRetentionTest test)
        {
            try
            {
                if (!test.Questions.Any())
                {
                    // Get ALL visual retention questions (fixed set - no randomization)
                    var questions = await _unitOfWork.Questions.GetByTypeAsync("visualretention");

                    foreach (var question in questions)
                    {
                        test.Questions.Add(question);
                    }

                    _logger.LogInformation("Loaded {QuestionCount} fixed Visual Retention questions for test {TestId}",
                        questions.Count(), test.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fixed questions for Visual Retention test {TestId}", test.Id);
                throw;
            }
        }
    }
}