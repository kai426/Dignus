using AutoMapper;
using Dignus.Candidate.Back.DTOs.Unified;
using Dignus.Candidate.Back.Exceptions;
using Dignus.Candidate.Back.Services.Interfaces;
using Dignus.Data.Models.Core;
using Dignus.Data.Repositories.Interfaces;

namespace Dignus.Candidate.Back.Services;

/// <summary>
/// Service for managing multiple-choice question responses
/// </summary>
public class QuestionResponseService : IQuestionResponseService
{
    private readonly ITestQuestionResponseRepository _responseRepo;
    private readonly ITestInstanceRepository _testInstanceRepo;
    private readonly ITestQuestionSnapshotRepository _questionSnapshotRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<QuestionResponseService> _logger;

    public QuestionResponseService(
        ITestQuestionResponseRepository responseRepo,
        ITestInstanceRepository testInstanceRepo,
        ITestQuestionSnapshotRepository questionSnapshotRepo,
        IMapper mapper,
        ILogger<QuestionResponseService> logger)
    {
        _responseRepo = responseRepo;
        _testInstanceRepo = testInstanceRepo;
        _questionSnapshotRepo = questionSnapshotRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<QuestionResponseDto>> SubmitAnswersAsync(Guid testId, Guid candidateId, List<QuestionAnswerSubmission> answers)
    {
        _logger.LogInformation("Submitting {AnswerCount} answers for test {TestId}, candidate {CandidateId}",
            answers.Count, testId, candidateId);

        // 1. Validate test and ownership
        var test = await _testInstanceRepo.GetByIdWithSnapshotsAsync(testId);
        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        if (test.Status == Data.Models.TestStatus.Submitted)
        {
            throw new InvalidOperationException("Cannot submit answers to an already submitted test");
        }

        // 2. Create response entities
        var responses = new List<TestQuestionResponse>();

        foreach (var answer in answers)
        {
            // Validate question exists
            var questionSnapshot = await _questionSnapshotRepo.GetByIdAsync(answer.QuestionSnapshotId);
            if (questionSnapshot == null)
            {
                _logger.LogWarning("Question snapshot {QuestionSnapshotId} not found, skipping", answer.QuestionSnapshotId);
                continue;
            }

            // Verify question belongs to this test
            if (questionSnapshot.TestInstanceId != testId)
            {
                _logger.LogWarning("Question snapshot {QuestionSnapshotId} does not belong to test {TestId}, skipping",
                    answer.QuestionSnapshotId, testId);
                continue;
            }

            // Check if response already exists
            var existingResponse = await _responseRepo.GetBySnapshotIdAsync(answer.QuestionSnapshotId);
            if (existingResponse != null && existingResponse.CandidateId == candidateId)
            {
                _logger.LogInformation("Updating existing response for question {QuestionSnapshotId}", answer.QuestionSnapshotId);

                // Update existing response
                existingResponse.SelectedAnswersJson = System.Text.Json.JsonSerializer.Serialize(answer.SelectedAnswers);
                existingResponse.ResponseTimeMs = answer.ResponseTimeMs;
                existingResponse.AnsweredAt = DateTimeOffset.UtcNow;

                await _responseRepo.UpdateAsync(existingResponse);
                responses.Add(existingResponse);
            }
            else
            {
                // Create new response
                var response = new TestQuestionResponse
                {
                    Id = Guid.NewGuid(),
                    TestInstanceId = testId,
                    CandidateId = candidateId,
                    QuestionSnapshotId = answer.QuestionSnapshotId,
                    SelectedAnswersJson = System.Text.Json.JsonSerializer.Serialize(answer.SelectedAnswers),
                    ResponseTimeMs = answer.ResponseTimeMs,
                    AnsweredAt = DateTimeOffset.UtcNow
                };

                await _responseRepo.AddAsync(response);
                responses.Add(response);
            }
        }

        await _responseRepo.SaveAsync();

        _logger.LogInformation("Successfully saved {ResponseCount} answers for test {TestId}", responses.Count, testId);

        return _mapper.Map<List<QuestionResponseDto>>(responses);
    }

    public async Task<List<QuestionResponseDto>> GetTestResponsesAsync(Guid testId, Guid candidateId)
    {
        var test = await _testInstanceRepo.GetByIdAsync(testId);
        if (test == null)
        {
            throw new NotFoundException("TestInstance", testId);
        }

        if (test.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Test {testId} does not belong to candidate {candidateId}");
        }

        var responses = await _responseRepo.GetByTestIdAsync(testId);
        return _mapper.Map<List<QuestionResponseDto>>(responses);
    }

    public async Task<QuestionResponseDto> GetResponseAsync(Guid responseId, Guid candidateId)
    {
        var response = await _responseRepo.GetByIdAsync(responseId);
        if (response == null)
        {
            throw new NotFoundException("QuestionResponse", responseId);
        }

        if (response.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Question response {responseId} does not belong to candidate {candidateId}");
        }

        return _mapper.Map<QuestionResponseDto>(response);
    }

    public async Task DeleteResponseAsync(Guid responseId, Guid candidateId)
    {
        var response = await _responseRepo.GetByIdAsync(responseId);
        if (response == null)
        {
            throw new NotFoundException("QuestionResponse", responseId);
        }

        if (response.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Question response {responseId} does not belong to candidate {candidateId}");
        }

        // Check if test is still in progress
        var test = await _testInstanceRepo.GetByIdAsync(response.TestInstanceId);
        if (test?.Status == Data.Models.TestStatus.Submitted)
        {
            throw new InvalidOperationException("Cannot delete responses from a submitted test");
        }

        await _responseRepo.DeleteAsync(responseId);
        await _responseRepo.SaveAsync();

        _logger.LogInformation("Question response {ResponseId} deleted", responseId);
    }

    public async Task<QuestionResponseDto> UpdateResponseAsync(Guid responseId, Guid candidateId, QuestionAnswerSubmission updatedAnswer)
    {
        var response = await _responseRepo.GetByIdAsync(responseId);
        if (response == null)
        {
            throw new NotFoundException("QuestionResponse", responseId);
        }

        if (response.CandidateId != candidateId)
        {
            throw new UnauthorizedAccessException($"Question response {responseId} does not belong to candidate {candidateId}");
        }

        // Check if test is still in progress
        var test = await _testInstanceRepo.GetByIdAsync(response.TestInstanceId);
        if (test?.Status == Data.Models.TestStatus.Submitted)
        {
            throw new InvalidOperationException("Cannot update responses in a submitted test");
        }

        // Update response
        response.SelectedAnswersJson = System.Text.Json.JsonSerializer.Serialize(updatedAnswer.SelectedAnswers);
        response.ResponseTimeMs = updatedAnswer.ResponseTimeMs;
        response.AnsweredAt = DateTimeOffset.UtcNow;

        await _responseRepo.UpdateAsync(response);
        await _responseRepo.SaveAsync();

        _logger.LogInformation("Question response {ResponseId} updated", responseId);

        return _mapper.Map<QuestionResponseDto>(response);
    }
}
