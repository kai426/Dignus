using Dignus.Candidate.Back.DTOs.Unified;

namespace Dignus.Candidate.Back.Services.Interfaces;

/// <summary>
/// Service for managing multiple-choice question responses
/// </summary>
public interface IQuestionResponseService
{
    /// <summary>
    /// Submits a batch of question responses for a test
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <param name="answers">List of question answers to submit</param>
    /// <returns>List of created question response DTOs</returns>
    Task<List<QuestionResponseDto>> SubmitAnswersAsync(Guid testId, Guid candidateId, List<QuestionAnswerSubmission> answers);

    /// <summary>
    /// Gets all question responses for a specific test
    /// </summary>
    /// <param name="testId">Test instance ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <returns>List of question responses</returns>
    Task<List<QuestionResponseDto>> GetTestResponsesAsync(Guid testId, Guid candidateId);

    /// <summary>
    /// Gets a specific question response by ID
    /// </summary>
    /// <param name="responseId">Question response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <returns>Question response DTO</returns>
    Task<QuestionResponseDto> GetResponseAsync(Guid responseId, Guid candidateId);

    /// <summary>
    /// Deletes a question response (only allowed before test submission)
    /// </summary>
    /// <param name="responseId">Question response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    Task DeleteResponseAsync(Guid responseId, Guid candidateId);

    /// <summary>
    /// Updates a question response (only allowed before test submission)
    /// </summary>
    /// <param name="responseId">Question response ID</param>
    /// <param name="candidateId">Candidate ID for IDOR protection</param>
    /// <param name="updatedAnswer">Updated answer data</param>
    /// <returns>Updated question response DTO</returns>
    Task<QuestionResponseDto> UpdateResponseAsync(Guid responseId, Guid candidateId, QuestionAnswerSubmission updatedAnswer);
}
